/* ------------------------------
   基本変数
------------------------------ */
const svg = document.getElementById("drawingArea");
const toolButtons = document.querySelectorAll("button[data-tool]");
const colorButtons = document.querySelectorAll("button[data-color]");
const fillButtons = document.querySelectorAll("button[data-fill]");
const fontButtons = document.querySelectorAll("button[data-font]");
const clearButton = document.getElementById("clear");

let currentTool = "freehand";
let currentColor = "#000000";
let fillMode = "fill";
let currentFontSize = 24;

let isDrawing = false;
let currentPath = null;
let selectedElement = null;
let offsetX = 0;
let offsetY = 0;

/* ★追加：ダブルクリック時の二重発火防止 */
let blockPointerDown = false;

/* ------------------------------
   active クラス反映
------------------------------ */
function activate(buttons, btn) {
    buttons.forEach(b => b.classList.remove("active"));
    btn.classList.add("active");
}

toolButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        activate(toolButtons, btn);
        currentTool = btn.dataset.tool;
    });
});

colorButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        activate(colorButtons, btn);
        currentColor = btn.dataset.color;
    });
});

fillButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        activate(fillButtons, btn);
        fillMode = btn.dataset.fill;
    });
});

fontButtons.forEach(btn => {
    btn.addEventListener("click", () => {
        activate(fontButtons, btn);
        currentFontSize = parseInt(btn.dataset.font);
    });
});

/* ------------------------------
   SVG 座標取得
------------------------------ */
function getSvgPoint(evt) {
    const rect = svg.getBoundingClientRect();
    const clientX = evt.touches ? evt.touches[0].clientX : evt.clientX;
    const clientY = evt.touches ? evt.touches[0].clientY : evt.clientY;
    return { x: clientX - rect.left, y: clientY - rect.top };
}

/* ------------------------------
   図形追加
------------------------------ */
function addShapeAt(tool, x, y) {
    const size = 60;
    let element;

    if (tool === "rect") {
        element = document.createElementNS("http://www.w3.org/2000/svg", "rect");
        element.setAttribute("x", x - size / 2);
        element.setAttribute("y", y - size / 2);
        element.setAttribute("width", size);
        element.setAttribute("height", size);
    }

    if (tool === "circle") {
        element = document.createElementNS("http://www.w3.org/2000/svg", "circle");
        element.setAttribute("cx", x);
        element.setAttribute("cy", y);
        element.setAttribute("r", size / 2);
    }

    if (tool === "triangle") {
        const half = size / 2;
        const p1 = `${x},${y - half}`;
        const p2 = `${x - half},${y + half}`;
        const p3 = `${x + half},${y + half}`;
        element = document.createElementNS("http://www.w3.org/2000/svg", "polygon");
        element.setAttribute("points", `${p1} ${p2} ${p3}`);
    }

    if (fillMode === "fill") {
        element.setAttribute("fill", currentColor);
    } else {
        element.setAttribute("fill", "none");
    }
    element.setAttribute("stroke", currentColor);
    element.setAttribute("stroke-width", "2");

    makeDraggable(element);
    svg.appendChild(element);
}

/* ------------------------------
   ★ テキスト入力
------------------------------ */
svg.addEventListener("dblclick", startTextEdit);

function startTextEdit(evt) {
    blockPointerDown = true;

    const { x, y } = getSvgPoint(evt);

    /* ★既存テキストを編集する場合 */
    let target = evt.target;
    let editingExisting = false;
    let originalText = "";
    let textElement = null;

    if (target.tagName === "text") {
        editingExisting = true;
        textElement = target;
        originalText = target.textContent;
    }

    /* ★textarea を作成 */
    const textarea = document.createElement("textarea");
    textarea.className = "text-editor";
    textarea.style.left = (evt.clientX) + "px";
    textarea.style.top = (evt.clientY) + "px";
    textarea.style.fontSize = currentFontSize + "px";
    textarea.style.color = currentColor;
    textarea.style.lineHeight = "1.2em";

    textarea.value = editingExisting ? originalText : "";

    document.body.appendChild(textarea);
    textarea.focus();

    /* ★Shift+Enter → 改行、Enter → 確定 */
    textarea.addEventListener("keydown", e => {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            textarea.blur();
        }
    });

    /* ★確定処理 */
    textarea.addEventListener("blur", () => {
        const value = textarea.value;
        document.body.removeChild(textarea);

        if (value.trim() === "") {
            blockPointerDown = false;
            return;
        }

        if (editingExisting) {
            textElement.textContent = value;
            textElement.setAttribute("fill", currentColor);
            textElement.setAttribute("font-size", currentFontSize);
            blockPointerDown = false;
            return;
        }

        /* ★新規テキストを SVG に描画 */
        const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
        text.setAttribute("x", x);
        text.setAttribute("y", y);
        text.setAttribute("fill", currentColor);
        text.setAttribute("font-size", currentFontSize);
        text.textContent = value;

        makeDraggable(text);
        svg.appendChild(text);

        blockPointerDown = false;
    });
}

/* ------------------------------
   テキスト以外のドラッグ処理
------------------------------ */
function makeDraggable(el) {
    el.addEventListener("mousedown", startDrag);
    el.addEventListener("touchstart", startDrag, { passive: false });
}

function startDrag(evt) {
    evt.preventDefault();
    selectedElement = evt.target;

    const pos = getSvgPoint(evt);

    if (selectedElement.tagName === "rect") {
        offsetX = pos.x - parseFloat(selectedElement.getAttribute("x"));
        offsetY = pos.y - parseFloat(selectedElement.getAttribute("y"));
    }
    if (selectedElement.tagName === "circle") {
        offsetX = pos.x - parseFloat(selectedElement.getAttribute("cx"));
        offsetY = pos.y - parseFloat(selectedElement.getAttribute("cy"));
    }
    if (selectedElement.tagName === "text") {
        offsetX = pos.x - parseFloat(selectedElement.getAttribute("x"));
        offsetY = pos.y - parseFloat(selectedElement.getAttribute("y"));
    }
    if (selectedElement.tagName === "polygon") {
        offsetX = pos.x;
        offsetY = pos.y;
    }

    window.addEventListener("mousemove", drag);
    window.addEventListener("mouseup", endDrag);
    window.addEventListener("touchmove", drag, { passive: false });
    window.addEventListener("touchend", endDrag);
}

function drag(evt) {
    if (!selectedElement) return;

    const pos = getSvgPoint(evt);

    if (evt.shiftKey) {
        resizeShape(selectedElement, pos);
        return;
    }

    if (selectedElement.tagName === "rect") {
        selectedElement.setAttribute("x", pos.x - offsetX);
        selectedElement.setAttribute("y", pos.y - offsetY);
    }

    if (selectedElement.tagName === "circle") {
        selectedElement.setAttribute("cx", pos.x - offsetX);
        selectedElement.setAttribute("cy", pos.y - offsetY);
    }

    if (selectedElement.tagName === "text") {
        selectedElement.setAttribute("x", pos.x - offsetX);
        selectedElement.setAttribute("y", pos.y - offsetY);
    }

    if (selectedElement.tagName === "polygon") {
        const dx = pos.x - offsetX;
        const dy = pos.y - offsetY;
        offsetX = pos.x;
        offsetY = pos.y;

        const points = selectedElement
            .getAttribute("points")
            .split(" ")
            .map(p => {
                const [px, py] = p.split(",").map(Number);
                return `${px + dx},${py + dy}`;
            })
            .join(" ");

        selectedElement.setAttribute("points", points);
    }
}

function endDrag() {
    selectedElement = null;
    window.removeEventListener("mousemove", drag);
    window.removeEventListener("mouseup", endDrag);
}

/* ------------------------------
   サイズ変更
------------------------------ */
function resizeShape(el, pos) {
    if (el.tagName === "rect") {
        el.setAttribute("width", Math.abs(pos.x - parseFloat(el.getAttribute("x"))));
        el.setAttribute("height", Math.abs(pos.y - parseFloat(el.getAttribute("y"))));
    }

    if (el.tagName === "circle") {
        const cx = parseFloat(el.getAttribute("cx"));
        const cy = parseFloat(el.getAttribute("cy"));
        const r = Math.sqrt((pos.x - cx) ** 2 + (pos.y - cy) ** 2);
        el.setAttribute("r", r);
    }
}

/* ------------------------------
   フリーハンド
------------------------------ */
function onPointerDown(evt) {
    evt.preventDefault();

    if (blockPointerDown) return;
    if (evt.target !== svg) return;

    const { x, y } = getSvgPoint(evt);

    if (currentTool === "freehand") {
        isDrawing = true;
        currentPath = document.createElementNS("http://www.w3.org/2000/svg", "path");
        currentPath.setAttribute("d", `M ${x} ${y}`);
        currentPath.setAttribute("stroke", currentColor);
        currentPath.setAttribute("stroke-width", "2");
        currentPath.setAttribute("fill", "none");
        svg.appendChild(currentPath);
    } else {
        addShapeAt(currentTool, x, y);
    }
}

function onPointerMove(evt) {
    if (!isDrawing) return;
    const { x, y } = getSvgPoint(evt);
    const d = currentPath.getAttribute("d");
    currentPath.setAttribute("d", d + ` L ${x} ${y}`);
}

function onPointerUp() {
    isDrawing = false;
    currentPath = null;
}

/* ------------------------------
   イベント登録
------------------------------ */
svg.addEventListener("mousedown", onPointerDown);
svg.addEventListener("mousemove", onPointerMove);
window.addEventListener("mouseup", onPointerUp);

svg.addEventListener("touchstart", onPointerDown, { passive: false });
svg.addEventListener("touchmove", onPointerMove, { passive: false });
window.addEventListener("touchend", onPointerUp);

/* ------------------------------
   クリア
------------------------------ */
clearButton.addEventListener("click", () => {
    while (svg.firstChild) svg.removeChild(svg.firstChild);
});
