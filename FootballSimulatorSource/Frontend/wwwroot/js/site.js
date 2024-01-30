import * as game from './game.js';
import * as cookies from './cookies.js';

let coachId = cookies.getCookie("coachId");
let matchId = cookies.getCookie("matchId");

if (coachId === "" || matchId === "") {
    let username = prompt("You are not logged in in the system. Please provide your username.");
    let password = prompt("Please provide your password.")
    // Send credentials to frontend and obtain coachId and matchId.
    const response = await fetch("/Login",{
        headers: {
          'Accept': 'application/json',
          'Content-Type': 'application/json'
        },
        method: "POST",
        body: JSON.stringify({username: username, password: password})
    });
    if (response.ok) {
        const jsonResponse = await response.json();
        const userData = JSON.parse(jsonResponse);
        coachId = userData.coachId;
        matchId = userData.matchId;
        cookies.setCookie("coachId", coachId, 30);
        cookies.setCookie("matchId", matchId, 30);
    }
}

await game.start(coachId, matchId);