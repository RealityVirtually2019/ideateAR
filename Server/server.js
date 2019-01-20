var express = require('express');
var path = require('path');
var favicon = require('serve-favicon');
var logger = require('morgan');
var cookieParser = require('cookie-parser');
var bodyParser = require('body-parser');
var expressWs = require('express-ws');

var ews = expressWs(express());
var app = ews.app;

// Set up the '/ws' resource to handle web socket connections
var sockets = [];
var nextId = 100;

// Set up the '/ws' resource to handle web socket connections
app.ws('/ws', function(ws, req) {
  nextId++;
  ws.localId = nextId;
  ws.send("000|l|Your socket id is " + ws.localId);
  // A message has been received from a client
  ws.on('message', function(msg) {
    var clients = ews.getWss('/ws').clients;
    
    console.log(ws.localId + " > " + msg);
    var targetId = null;
    
    if(msg.startsWith(">"))
    {
      targetId = msg.substring(1, msg.indexOf("|"));
      msg = msg.substring(msg.indexOf("|") + 1);
    }
    
    msg = ws.localId + "|" + msg;

    // Broadcast it to all other clients
    clients.forEach(c=> {
      if(targetId) 
      {        
        if(c.localId == targetId)
        {
          c.send(msg);          
        }
      }
      else
      {
        if(c != ws) c.send(msg);
      }
    });
  });
});


// uncomment after placing your favicon in /public
//app.use(favicon(path.join(__dirname, 'public', 'favicon.ico')));
app.use(logger('dev'));
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: false }));
app.use(cookieParser());

app.get("/scan", function(req, res)
{
  var id = req.query.id;
  
  console.log("Got a scan request: ", id);
  res.send("Thank you for the lovely info: " + id);
  res.end();
  
  var sockets = ews.getWss('/ws').clients;
  sockets.forEach(c=> {
    console.log("Sending out the info on the scan");
      c.send("000|media|model|" + id);
  });
});

app.use(express.static(path.join(__dirname, 'public')));



// catch 404 and forward to error handler
app.use(function(req, res, next) {
  var err = new Error('Not Found');
  err.status = 404;
  next(err);
});

// error handler
app.use(function(err, req, res, next) {
  console.log("error", err);
  if (err.status)
    res.sendStatus(err.status);
  else
    res.sendStatus(500);
});


app.listen(process.env.PORT);
console.log('Webserver started');
module.exports = app;
