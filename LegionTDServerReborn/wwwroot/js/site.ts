interface Duel {
    matchId : number,
    order : number,
    winner : number,
    timeStamp : number
}

interface PlayerMatchData {
    playerId : number,
    matchId : number,
    team : number,
    abandoned : boolean,
    ratingChange : number,
    earnedGold : number,
    earnedTangos : number,
    fractionName : string,
    won : boolean,
    wonDuels : number,
    lostDuels : number,
    experience : number,
    kills : number,
    leaks : number,
    builds : number,
    sends : number
}

interface Match {
    matchId : number,
    date : string,
    isTraining : boolean,
    winner : number,
    lastWave : number,
    duration : number,
    duels : Duel[],
    playerDatas : PlayerMatchData[]
}

function MatchToTableEntry(match : Match) : string {
    return "<tr class='clickable-row' data-href='/Match/" + match.matchId + "'>" +
        ToCells([match.matchId, 
            FormatDate(new Date(match.date)), 
            FormatDuration(match.duration),
            GetWinnerText(match),
            match.lastWave
        ]) +
        "</tr>";
}

function ToCells(entries : any[]) : string {
    var result = "";
    for (let value of entries) {
        result += "<td>"+ value + "</td>";
    }
    return result;
}

function MatchToMatchInfoCells(match : Match) : string {
    return ToMatchInfoCell("Date", FormatDate(new Date(match.date))) + 
        ToMatchInfoCell("Duration", FormatDuration(match.duration)) +
        ToMatchInfoCell("Winner", GetWinnerText(match)) +
        ToMatchInfoCell("Last Wave", match.lastWave);
}

function ToMatchInfoCell(title : any, value : any) : string {
    return "<div class='col-md-3 col-sm-3 col-xs-6 match-info'>" +
        "<p class='match-info-title'>" + title + "</p>" +
        "<p class='match-info-value'>" + value + "</p>" +
        "</div>";
}

function GetWinnerText(match : Match) : string {
    return (match.winner == 2 ? "<span class='radiant'>Radiant</td>" : "<span class='dire'>Dire</span>");
}

function FormatDuration(duration : number) : string {
    return padLeft(Math.floor(duration/60), 2, 0) + ":" + padLeft(Math.round(duration) % 60, 2, 0);
}

function FormatDate(date : Date) : string {
    return padLeft(date.getDay(), 2, 0) + "." + padLeft(date.getMonth(), 2, 0) + "." + date.getFullYear() + " " + padLeft(date.getHours(), 2, 0) + ":" + padLeft(date.getMinutes(), 2, 0);
}

function padLeft(nr : any, n : number, str : any) : string{
    return Array(n-String(nr).length+1).join(str||'0')+nr;
}
