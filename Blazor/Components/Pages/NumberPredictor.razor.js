window.canvas = {
    drawCircle: function (canvas, x, y, r, color) {
        const ctx = canvas.getContext("2d");
        ctx.fillStyle = color;
        ctx.beginPath();
        ctx.arc(x, y, r, 0, Math.PI * 2);
        ctx.fill();
    },

    clear: function (canvas) {
        const ctx = canvas.getContext("2d");
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    },

    savePng: function (canvas, filename) {
        const link = document.createElement("a");
        link.download = filename;
        link.href = canvas.toDataURL("image/png");
        link.click();
    },

    disableContext: function () {
        document.addEventListener("contextmenu", e => e.preventDefault());
    }
};