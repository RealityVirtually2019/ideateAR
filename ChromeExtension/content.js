var socket = null;
var overElem = null;
var posX = null;
var posY = null;
var selElem = null; // store the currently selected element
var origBorder = "";    // stores the border settings of the selected element

if (document.readyState != 'loading') ready();
else document.addEventListener('DOMContentLoaded', ready);

function ready() 
{
  const url = 'wss://ideatear.glitch.me/ws';
  socket = new ReconnectingWebsocket(url);

  socket.onopen = function(evt) 
  {
    console.log('Web socket opened: ' + url);
  };

  socket.onmessage = function(evt) 
  {
    console.log("received: " + evt.data); 
    parseMessage(evt.data);
  };
}

function enroll()
{
    if(socket)
    {
        socket.send("enroll|static|drawing");
        console.log('sent enroll|static|drawing');
    }
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

      OnPositionChanged(posX, posY);

      console.log('mouse x: ' + posX + ', y: ' + posY);

      break;
    case "l": //Log
      console.log("Log message from ", sender, ": ", tokens[2]);
      break;
    case "q": //Query
      console.log("Received query from ", sender);

      if(overElem && overElem.src)
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
  
  //var data = overElem.toDataURL();
  var data = overElem.src;
  socket.send(">" + requestor + "|" + data);

  console.log('sent image: ' + data);
}

function OnPositionChanged (posX, posY) {

    overElem = document.elementFromPoint(posX, posY);
    
    if (overElem && overElem.tagName === undefined) {   // in case of text nodes (Opera)
        overElem = overElem.parentNode; // the parent node will be selected
    }

    if (selElem) {  // if there was previously selected element
        if (selElem == overElem) {  // if mouse is over the previously selected element
            return; // does not need to update the selection border
        }
        selElem.style.border = origBorder;  // set border to the stored value
        selElem = null;
    }
        // the body and the html tag won't be selected
    if (overElem && overElem.tagName.toLowerCase () != "body" && overElem.tagName.toLowerCase () != "html") {
        selElem = overElem; // stores the selected element
        origBorder = overElem.style.border; // stores the border settings of the selected element
        overElem.style.border = "3px solid red";    // draws selection border
    }
}