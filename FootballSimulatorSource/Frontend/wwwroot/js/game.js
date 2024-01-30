import { Vector2 } from './Vector2.js';

let coachId = "";
let matchId = "";

// The app object that is common to the client.
let app;
let fieldContainer;
let players = [];
let ball = {
    position: new Vector2(0,0),
    graphic: new PIXI.Graphics()
    .beginFill(0xDDDDDD)
    .drawCircle(0, 0, 0.5)
    .endFill()
};
let teams = [
    { 
        id: null,
        name: "Team 1",
        color: 0xFF0000,
    },
    {
        id: null,
        name: "Team 2",
        color: 0x0000FF
    }
];
let teamDict = {};

async function start(coachIdParam, matchIdParam) {
    coachId = coachIdParam;
    matchId = matchIdParam;

    app = new PIXI.Application({ background: '#888888', resizeTo: window });
    document.body.appendChild(app.view);
        
    drawField();
    await getMatchGamestate();
    addPlayerGraphicsToField();
    updateEntities();
}

function drawField() {
    fieldContainer = new PIXI.Container();
    // Field lines
    const lines = new PIXI.Graphics()
        .lineStyle(0.1, 0xFFFFFF, 1)
        .beginFill(0x00AA00)
        .drawRect(-30, -20, 60, 40)
        .endFill()
        .lineTo(0, 20)
        .lineTo(-30, 20)
        .lineTo(-30, -20)
        .lineTo(0, -20)
        .lineTo(0, 20)
        .lineTo(30, 20)
        .lineTo(30, -20)
        .lineTo(0, -20)
        .closePath();
    // Central Circle
    lines.drawCircle(0, 0, 5);
    // Goals
    lines.drawRect(-34, -4, 4, 8);
    lines.drawRect(30, -4, 4, 8);

    fieldContainer.addChild(lines);

    // Ball
    fieldContainer.addChild(ball.graphic);
    
    fieldContainer.pivot.x = 0;
    fieldContainer.pivot.y = 0;
    fieldContainer.x = app.screen.width / 2;
    fieldContainer.y = app.screen.height / 2;
    fieldContainer.scale.x = 10;
    fieldContainer.scale.y = 10;

    app.stage.addChild(fieldContainer);
   

    fieldContainer.eventMode = 'static';
    fieldContainer.on("pointerdown", async (event) => {        
        var offsetClick = new Vector2(event.globalX - fieldContainer.x, event.globalY - fieldContainer.y);
        var fieldScaleClick = offsetClick.scale(1 / fieldContainer.scale.x);
        console.log("You clicked at " + fieldScaleClick);
        simulateMatch(fieldScaleClick);
    });
}

function addPlayerGraphicsToField(){    
    for (let player of players){
        fieldContainer.addChild(player.graphic);        
    }
}

function removePlayerGraphicsFromField(){    
    for (let player of players){
        fieldContainer.removeChild(player.graphic);        
    }
}

function updateEntities(){
    // Update the position of each player's graphic based on the player's position.
    for (let player of players){
        player.graphic.x = player.position.x;
        player.graphic.y = player.position.y;
    }    
    // Update the ball's position
    ball.graphic.x = ball.position.x;
    ball.graphic.y = ball.position.y;
    ball.graphic.zIndex = 1000;
    fieldContainer.sortChildren();
}

function updateMatch(matchData) {
    // Delete the graphic representation of the players because they will be recreated.
    removePlayerGraphicsFromField();

    // Get team information
    const homeTeam = matchData.homeTeam;
    const awayTeam = matchData.awayTeam;
    teams[0].id = homeTeam.id;
    teams[0].name = homeTeam.name;
    teams[1].id = awayTeam.id;
    teams[1].name = awayTeam.name;
    teamDict[homeTeam.id] = teams[0];
    teamDict[awayTeam.id] = teams[1];

    // Update player information
    players = matchData.players
        .map(p => ({
            id: p.id,
            team: teamDict[p.teamId],
            position: new Vector2(p.posX, p.posY),
            graphic: new PIXI.Graphics()
                .beginFill(teamDict[p.teamId].color)
                .drawCircle(0, 0, 1)
                .endFill()
        }));

    // Update the ball position
    ball.position = new Vector2(matchData.match.ballPosX, matchData.match.ballPosY);

    // Re-add the players to the field container because they have been recreated.
    addPlayerGraphicsToField();
    updateEntities();
}

async function simulateMatch(clickPosition) {
    try {
        // Fetch game state
        const response = await fetch(`/SimulateMatchClick?clickX=${clickPosition.x}&clickY=${clickPosition.y}&coachId=${coachId}&matchId=${matchId}`, { cache: 'no-cache' });
        // If it answers, process the response
        if (response.ok) {
            const jsonResponse = await response.json();
            const matchData = JSON.parse(jsonResponse);
            updateMatch(matchData); 
            console.log(`The ball is at ${ball.position}`);
            for(let event of matchData.events){
                console.log(event);
            }
        }
    }
    catch (error) {
        console.log(error);
    }        
}

async function getMatchGamestate() {
    // Interact with MatchCalculator container to obtain the gamestate of this match
    try {
        // Fetch game state
        const response = await fetch(`/GetGameState?matchId=${matchId}`, { cache: 'no-cache' });
        // If it answers, process the response
        if (response.ok) {
            const jsonResponse = await response.json();            
            const matchData = JSON.parse(jsonResponse);
            updateMatch(matchData);
        }
    }
    catch (error) {
        console.log(error);
    }        
}

export { start };