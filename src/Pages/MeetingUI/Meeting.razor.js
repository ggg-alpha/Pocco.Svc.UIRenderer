// ===============================
// ES6 Module for Blazor
// ===============================

// モジュール内で保持する状態
let rooms = JSON.parse(localStorage.getItem("rooms") || `["Room A","Room B","Room C"]`);
let currentRoom = null;
let tempParticipants = [];
let reservations = JSON.parse(localStorage.getItem("reservations") || "{}");

// -------------------------
// 保存処理
// -------------------------
function saveRooms() {
    localStorage.setItem("rooms", JSON.stringify(rooms));
}

function saveData() {
    localStorage.setItem("reservations", JSON.stringify(reservations));
}

// -------------------------
// 部屋追加
// -------------------------
export function addRoom() {
    const name = document.getElementById("new-room-name").value.trim();
    if (!name) return alert("部屋名を入力してください");

    if (rooms.includes(name)) return alert("同じ名前の部屋があります");

    rooms.push(name);
    saveRooms();
    renderRoomList();

    document.getElementById("new-room-name").value = "";
}

// -------------------------
// ミーティングURL生成
// -------------------------
function generateURL() {
    const uuid = crypto.randomUUID();
    return "https://meeting.example.com/" + uuid;
}

// -------------------------
// 部屋一覧表示
// -------------------------
export function renderRoomList() {
    const list = document.getElementById("room-list");
    list.innerHTML = "";

    rooms.forEach(roomName => {
        const roomRes = reservations[roomName] || [];
        const now = new Date();
        const busy = roomRes.some(r => new Date(r.start) > now);

        const card = document.createElement("div");
        card.className = "room-card " + (busy ? "busy" : "available");
        card.onclick = () => selectRoom(roomName);

        card.innerHTML = `
            <h3>${roomName}</h3>
            <div class="room-info">
                状態: ${busy ? "使用中" : "空き"}
            </div>
        `;

        list.appendChild(card);
    });
}

// -------------------------
// 部屋選択
// -------------------------
export function selectRoom(roomName) {
    currentRoom = roomName;
    tempParticipants = [];
    document.getElementById("room-title").textContent = roomName;
    renderParticipants();
    renderReservations();
}

// -------------------------
// 参加者追加
// -------------------------
export function addParticipant() {
    const tag = document.getElementById("tag").value;
    const p = document.getElementById("participant").value.trim();

    if (!p) return;

    if (tag === "1on1" && tempParticipants.length >= 1) {
        alert("1on1では参加者は1人までです");
        return;
    }

    tempParticipants.push(p);
    document.getElementById("participant").value = "";
    renderParticipants();
}

function renderParticipants() {
    const view = document.getElementById("participant-list-view");
    view.innerHTML = tempParticipants.length === 0
        ? "<p>参加者なし</p>"
        : tempParticipants.map(p => "・" + p).join("<br>");
}

// -------------------------
// ミーティング作成
// -------------------------
export function reserve() {
    if (!currentRoom) return alert("部屋を選択してください");

    const user = document.getElementById("username").value;
    const date = document.getElementById("date").value;
    const start = document.getElementById("start-time").value;
    const end = document.getElementById("end-time").value;
    const tag = document.getElementById("tag").value;

    if (!user || !date || !start || !end)
        return alert("主催者名・日付・時間を入力してください");

    if (tempParticipants.length === 0) {
        return alert("参加者がいないミーティングは作成できません");
    }

    if (tag === "1on1" && tempParticipants.length > 1) {
        return alert("1on1では参加者は1人までです");
    }

    const startDateTime = `${date}T${start}`;
    const endDateTime = `${date}T${end}`;

    if (new Date(startDateTime) >= new Date(endDateTime))
        return alert("終了時間は開始時間より後にしてください");

    if (!reservations[currentRoom]) reservations[currentRoom] = [];

    const overlap = reservations[currentRoom].some(r =>
        !(new Date(r.end) <= new Date(startDateTime) ||
          new Date(r.start) >= new Date(endDateTime))
    );

    if (overlap) return alert("この時間帯はすでに予約があります");

    reservations[currentRoom].push({
        user,
        start: startDateTime,
        end: endDateTime,
        tag,
        url: generateURL(),
        participants: [...tempParticipants]
    });

    reservations[currentRoom].sort((a, b) => new Date(a.start) - new Date(b.start));

    saveData();
    renderReservations();
    renderRoomList();

    document.getElementById("username").value = "";
    document.getElementById("date").value = "";
    tempParticipants = [];
    renderParticipants();
}

// -------------------------
// 予約削除
// -------------------------
export function deleteReservation(roomName, index) {
    reservations[roomName].splice(index, 1);
    saveData();
    renderReservations();
    renderRoomList();
}

// -------------------------
// 予約一覧表示
// -------------------------
export function renderReservations() {
    const list = document.getElementById("reservation-list");
    list.innerHTML = "";

    if (!currentRoom) return;

    const roomRes = reservations[currentRoom] || [];

    if (roomRes.length === 0) {
        list.innerHTML = "<p>ミーティングはありません</p>";
        return;
    }

    roomRes.forEach((r, i) => {
        const item = document.createElement("div");
        item.className = "reservation-item";

        item.innerHTML = `
            <strong>${r.start} 〜 ${r.end}</strong><br>
            主催者: ${r.user}<br>
            種類: ${r.tag}<br>
            URL: <a href="${r.url}" target="_blank">${r.url}</a><br>
            参加者: ${r.participants.join(", ") || "なし"}
            <button class="delete-btn" onclick="deleteReservation('${currentRoom}', ${i})">削除</button>
        `;

        list.appendChild(item);
    });
}

// -------------------------
// 時間選択（5分刻み）
// -------------------------
export function generateTimeOptions() {
    const startSel = document.getElementById("start-time");
    const endSel = document.getElementById("end-time");

    for (let h = 0; h < 24; h++) {
        for (let m = 0; m < 60; m += 5) {
            const hh = String(h).padStart(2, "0");
            const mm = String(m).padStart(2, "0");
            const t = `${hh}:${mm}`;
            const opt1 = document.createElement("option");
            const opt2 = document.createElement("option");
            opt1.value = opt2.value = t;
            opt1.textContent = opt2.textContent = t;
            startSel.appendChild(opt1);
            endSel.appendChild(opt2);
        }
    }
}

// -------------------------
// 初期化（Blazor から呼ぶ）
// -------------------------
export function init() {
    renderRoomList();
    generateTimeOptions();
}
