// DrawingTool.razor.js

export function init() {
    // ------------------------------
    // 基本変数
    // ------------------------------
    const svg = document.querySelector("#drawingArea");
    const toolButtons = document.querySelectorAll("button[data-tool]");
    const colorButtons = document.querySelectorAll("button[data-color]");
    const fillButtons = document.querySelectorAll("button[data-fill]");
    const fontButtons = document.querySelectorAll("button[data-font]");
    const clearButton = document.querySelector("#clear");

    if (!svg) {
        console.error("Drawing area not found. Make sure an element with id='drawingArea' exists.");
        return {
            dispose: () => { } // Return a no-op dispose function
        };
    }

    let currentTool = "freehand";
    let currentColor = "#000000";
    let fillMode = "fill";
    let currentFontSize = 24;

    let isDrawing = false;
    let currentPath = null;
    let selectedElement = null;
    let offsetX = 0;
    let offsetY = 0;

    let blockPointerDown = false;
    let activeTextarea = null;

    // ------------------------------
    // active クラス反映
    // ------------------------------
    function activate(buttons, btn) {
        buttons.forEach(b => b.classList.remove("active"));
        btn.classList.add("active");
    }

    function onToolButtonClick() {
        activate(toolButtons, this);
        currentTool = this.dataset.tool;
    }

    function onColorButtonClick() {
        activate(colorButtons, this);
        currentColor = this.dataset.color;
    }

    function onFillButtonClick() {
        activate(fillButtons, this);
        fillMode = this.dataset.fill;
    }

    function onFontButtonClick() {
        activate(fontButtons, this);
        currentFontSize = parseInt(this.dataset.font);
    }
    
    function onClearButtonClick() {
        while (svg.firstChild) svg.removeChild(svg.firstChild);
    }


    toolButtons.forEach(btn => btn.addEventListener("click", onToolButtonClick));
    colorButtons.forEach(btn => btn.addEventListener("click", onColorButtonClick));
    fillButtons.forEach(btn => btn.addEventListener("click", onFillButtonClick));
    fontButtons.forEach(btn => btn.addEventListener("click", onFontButtonClick));
    clearButton.addEventListener("click", onClearButtonClick);


    // ------------------------------
    // SVG 座標取得
    // ------------------------------
    function getSvgPoint(evt) {
        const rect = svg.getBoundingClientRect();
        const clientX = evt.touches ? evt.touches[0].clientX : evt.clientX;
        const clientY = evt.touches ? evt.touches[0].clientY : evt.clientY;
        return { x: clientX - rect.left, y: clientY - rect.top };
    }

    // ------------------------------
    // 図形追加
    // ------------------------------
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

    // ------------------------------
    // テキスト入力
    // ------------------------------
    function startTextEdit(evt) {
        blockPointerDown = true;

        const { x, y } = getSvgPoint(evt);
        let target = evt.target;
        let editingExisting = false;
        let originalText = "";
        let textElement = null;

        if (target.tagName === "text") {
            editingExisting = true;
            textElement = target;
            originalText = target.textContent;
        }

        const textarea = document.createElement("textarea");
        activeTextarea = textarea;
        textarea.className = "text-editor";
        textarea.style.left = (evt.clientX) + "px";
        textarea.style.top = (evt.clientY) + "px";
        textarea.style.fontSize = currentFontSize + "px";
        textarea.style.color = currentColor;
        textarea.style.lineHeight = "1.2em";

        textarea.value = editingExisting ? originalText : "";

        document.body.appendChild(textarea);
        textarea.focus();

        function handleTextKeyDown(e) {
            if (e.key === "Enter" && !e.shiftKey) {
                e.preventDefault();
                textarea.blur();
            }
        }
        
        function handleTextBlur() {
            const value = textarea.value;
            if (textarea.parentNode) {
                document.body.removeChild(textarea);
            }
            activeTextarea = null;

            if (value.trim() === "") {
                blockPointerDown = false;
                return;
            }

            if (editingExisting) {
                textElement.textContent = value;
                textElement.setAttribute("fill", currentColor);
                textElement.setAttribute("font-size", currentFontSize);
            } else {
                const text = document.createElementNS("http://www.w3.org/2000/svg", "text");
                text.setAttribute("x", x);
                text.setAttribute("y", y);
                text.setAttribute("fill", currentColor);
                text.setAttribute("font-size", currentFontSize);
                text.textContent = value;
                makeDraggable(text);
                svg.appendChild(text);
            }
            
            blockPointerDown = false;
            textarea.removeEventListener("keydown", handleTextKeyDown);
            textarea.removeEventListener("blur", handleTextBlur);
        }

        textarea.addEventListener("keydown", handleTextKeyDown);
        textarea.addEventListener("blur", handleTextBlur);
    }
    svg.addEventListener("dblclick", startTextEdit);


    // ------------------------------
    // ドラッグ処理
    // ------------------------------
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
        } else if (selectedElement.tagName === "circle") {
            offsetX = pos.x - parseFloat(selectedElement.getAttribute("cx"));
            offsetY = pos.y - parseFloat(selectedElement.getAttribute("cy"));
        } else if (selectedElement.tagName === "text") {
            offsetX = pos.x - parseFloat(selectedElement.getAttribute("x"));
            offsetY = pos.y - parseFloat(selectedElement.getAttribute("y"));
        } else if (selectedElement.tagName === "polygon") {
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
        evt.preventDefault();
        const pos = getSvgPoint(evt);

        if (evt.shiftKey) {
            resizeShape(selectedElement, pos);
            return;
        }

        if (selectedElement.tagName === "rect") {
            selectedElement.setAttribute("x", pos.x - offsetX);
            selectedElement.setAttribute("y", pos.y - offsetY);
        } else if (selectedElement.tagName === "circle") {
            selectedElement.setAttribute("cx", pos.x - offsetX);
            selectedElement.setAttribute("cy", pos.y - offsetY);
        } else if (selectedElement.tagName === "text") {
            selectedElement.setAttribute("x", pos.x - offsetX);
            selectedElement.setAttribute("y", pos.y - offsetY);
        } else if (selectedElement.tagName === "polygon") {
            const dx = pos.x - offsetX;
            const dy = pos.y - offsetY;
            offsetX = pos.x;
            offsetY = pos.y;
            const points = selectedElement.getAttribute("points").split(" ").map(p => {
                const [px, py] = p.split(",").map(Number);
                return `${px + dx},${py + dy}`;
            }).join(" ");
            selectedElement.setAttribute("points", points);
        }
    }

    function endDrag() {
        selectedElement = null;
        window.removeEventListener("mousemove", drag);
        window.removeEventListener("mouseup", endDrag);
        window.removeEventListener("touchmove", drag);
        window.removeEventListener("touchend", endDrag);
    }
    
    // ------------------------------
    // サイズ変更
    // ------------------------------
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

    // ------------------------------
    // フリーハンド
    // ------------------------------
    function onPointerDown(evt) {
        if (blockPointerDown) return;
        if (evt.target !== svg) return;
        evt.preventDefault();

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

    // ------------------------------
    // イベント登録
    // ------------------------------
    svg.addEventListener("mousedown", onPointerDown);
    svg.addEventListener("mousemove", onPointerMove);
    window.addEventListener("mouseup", onPointerUp);
    svg.addEventListener("touchstart", onPointerDown, { passive: false });
    svg.addEventListener("touchmove", onPointerMove, { passive: false });
    window.addEventListener("touchend", onPointerUp);

    // ------------------------------
    // クリーンアップ
    // ------------------------------
    return {
        dispose: () => {
            toolButtons.forEach(btn => btn.removeEventListener("click", onToolButtonClick));
            colorButtons.forEach(btn => btn.removeEventListener("click", onColorButtonClick));
            fillButtons.forEach(btn => btn.removeEventListener("click", onFillButtonClick));
            fontButtons.forEach(btn => btn.removeEventListener("click", onFontButtonClick));
            clearButton.removeEventListener("click", onClearButtonClick);
            
            svg.removeEventListener("dblclick", startTextEdit);
            
            svg.removeEventListener("mousedown", onPointerDown);
            svg.removeEventListener("mousemove", onPointerMove);
            window.removeEventListener("mouseup", onPointerUp);
            svg.removeEventListener("touchstart", onPointerDown);
            svg.removeEventListener("touchmove", onPointerMove);
            window.removeEventListener("touchend", onPointerUp);
            
            window.removeEventListener("mousemove", drag);
            window.removeEventListener("mouseup", endDrag);
            window.removeEventListener("touchmove", drag);
            window.removeEventListener("touchend", endDrag);

            if (activeTextarea && activeTextarea.parentNode) {
                activeTextarea.parentNode.removeChild(activeTextarea);
            }
        }
    };
}