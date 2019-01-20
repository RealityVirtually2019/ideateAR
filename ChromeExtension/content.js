var socket = null;
var foundEl = null;
var posX = null;
var posY = null;

if (document.readyState != 'loading') ready();
else document.addEventListener('DOMContentLoaded', ready);

function ready() 
{
  const url = 'wss://ideatear.glitch.me/ws';
  socket = new ReconnectingWebsocket(url);

  socket.onopen = function(evt) 
  {
    socket.send("enroll|static|drawing");
    console.log('Web socket opened: ' + url);
  };

  socket.onmessage = function(evt) 
  {
    console.log("received: " + evt.data); 
    parseMessage(evt.data);
  };
}

function parseMessage(msg)
{
  var tokens = msg.split("|");
  var sender = tokens[0];
  var command = tokens[1];

  switch(command)
  {
    case "mouse":

      var width = window.innerWidth || document.body.clientWidth;
      var height = window.innerHeight || document.body.clientHeight;

      posX = width * tokens[2];
      posY = height * tokens[3];

      foundEl = document.elementFromPoint(posX, posY);

      break;
    case "l": //Log
      console.log("Log message from ", sender, ": ", tokens[2]);
      break;
    case "q": //Query
      console.log("Received query from ", sender);
      
      var width = window.innerWidth || document.body.clientWidth;
      var height = window.innerHeight || document.body.clientHeight;

      //posX = width * 0.5;
      //posY = height * 0.7;

      foundEl = document.elementFromPoint(posX, posY);

      if(foundEl && foundEl.src)
      {
         sendImage(sender);
      }
      else
      {
         socket.send(">" + sender + "|media|null");

         console.log('img not found at x: ' + posX + ', y: ' + posY);
      }

      break;
  }
}

function sendImage(requestor)
{
  socket.send(">" + requestor + "|media|img"); //Let them know a picture is inbound
  
  //var data = foundEl.toDataURL();
  var data = foundEl.src;
  socket.send(">" + requestor + "|" + data);
}