var socket = null;

if (document.readyState != 'loading') ready();
else document.addEventListener('DOMContentLoaded', ready);

function ready() 
{
  const url = 'wss://' + location.host + '/ws';
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
  var screenType = getRadioValue("screentype");
  var contentType = getRadioValue("contenttype");
  send("enroll|" + screenType + "|" + contentType);
  
  document.querySelector("#splash").style.display = "none";
  switch(contentType)
  {
    case "drawing":
      document.querySelector("#drawing").style.display = "";
      break;
    default:
      document.querySelector("#otherTitle").innerText = contentType;
      document.querySelector("#other").style.display = "";
      break;
  }
}



function getRadioValue(name)
{
  var radios = document.getElementsByName(name);

  for (var i = 0, length = radios.length; i < length; i++)
  {
    if (radios[i].checked) return radios[i].value;
  }
}

function parseMessage(msg)
{
  var tokens = msg.split("|");
  var sender = tokens[0]
  var command = tokens[1];
  switch(command)
  {
    case "l": //Log
      console.log("Log message from ", sender, ": ", tokens[2]);
      break;
    case "q": //Query
      console.log("Received query from ", sender);
      sendImage(sender);
      break;
  }
}

function sendImage(requestor)
{
  socket.send(">" + requestor + "|media|img"); //Let them know a picture is inbound
  
  var data = canvasEl.toDataURL();
  socket.send(">" + requestor + "|" + data);
  
  clearCanvas();
}

function sendMessage()
{
  var txt = document.querySelector("#textBox").value;
  var target = document.querySelector("#target").value;
  if(target) target = ">" + target + "|";
  send(target + txt);
}

function sendObject(o) 
{
  send(JSON.stringify(o));
}

function send(str) {
  console.log(new Date().toLocaleTimeString() +  '> ' + str);
  socket.send(str);
}
