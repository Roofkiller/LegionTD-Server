function MatchToTableEntry(match) {
    return "<tr class='clickable-row' data-href='/Match/" + match.matchId + "'>" +
        ToCells([match.matchId,
            FormatDate(new Date(match.date)),
            FormatDuration(match.duration),
            GetWinnerText(match),
            match.lastWave
        ]) +
        "</tr>";
}
function ToCells(entries) {
    var result = "";
    for (var _i = 0, entries_1 = entries; _i < entries_1.length; _i++) {
        var value = entries_1[_i];
        result += "<td>" + value + "</td>";
    }
    return result;
}
function MatchToMatchInfoCells(match) {
    return ToMatchInfoCell("Date", FormatDate(new Date(match.date))) +
        ToMatchInfoCell("Duration", FormatDuration(match.duration)) +
        ToMatchInfoCell("Winner", GetWinnerText(match)) +
        ToMatchInfoCell("Last Wave", match.lastWave);
}
function ToMatchInfoCell(title, value) {
    return "<div class='col-md-3 col-sm-3 col-xs-6 match-info'>" +
        "<p class='match-info-title'>" + title + "</p>" +
        "<p class='match-info-value'>" + value + "</p>" +
        "</div>";
}
function GetWinnerText(match) {
    return (match.winner == 2 ? "<span class='radiant'>Radiant</td>" : "<span class='dire'>Dire</span>");
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
