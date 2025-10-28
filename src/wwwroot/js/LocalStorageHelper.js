window.onbeforeunload = function () {
    let connectedClients = localStorage.getItem("connectedClients");
    if (connectedClients === null) {
        connectedClients = "0";
    }
    let count = parseInt(connectedClients);
    count = Math.max(0, count - 1);
    localStorage.setItem("connectedClients", count.toString());
}
