function MatchToTableEntry(data) {
    return "<tr class='clickable-row' data-href='/index'>" +
        "<td>" + data.matchId + "</td>" +
        "<td>" + FormatDate(new Date(data.date)) + "</td>" +
        "<td>" + FormatDuration(data.duration) + "</td>" +
        "<td>" + (data.winner == 2 ? "Radiant" : "Dire") + "</td>" +
        "<td>" + data.lastWave + "</td>" +
        "</tr>";
}
function FormatDuration(duration) {
    return padLeft(Math.floor(duration / 60), 2, 0) + ":" + padLeft(Math.round(duration) % 60, 2, 0);
}
function FormatDate(date) {
    return padLeft(date.getDay(), 2, 0) + "." + padLeft(date.getMonth(), 2, 0) + "." + date.getFullYear() + " " + padLeft(date.getHours(), 2, 0) + ":" + padLeft(date.getMinutes(), 2, 0);
}
function padLeft(nr, n, str) {
    return Array(n - String(nr).length + 1).join(str || '0') + nr;
}
