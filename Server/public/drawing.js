var canvasEl = document.querySelector("#drawingCanvas");
var context = canvasEl.getContext("2d");

clearCanvas();

canvasEl.addEventListener("mousedown", function(e)
{
  startDraw(e.offsetX, e.offsetY);
});

canvasEl.addEventListener("mousemove", function(e)
{
  if(e.buttons == 1) addDraw(e.offsetX, e.offsetY);
});

canvasEl.addEventListener("mouseup", function(e)
{
  endDraw(e.offsetX, e.offsetY);
});

canvasEl.addEventListener("touchstart", function(e)
{
  var touch = e.changedTouches[0];
  var x = (touch.clientX - e.target.offsetLeft);
  var y = (touch.clientY - e.target.offsetTop);
  startDraw(x, y);
  e.preventDefault();
});

canvasEl.addEventListener("touchmove", function(e)
{
  var touch = e.changedTouches[0];
  var x = (touch.clientX - e.target.offsetLeft);
  var y = (touch.clientY - e.target.offsetTop);
  addDraw(x, y);
  e.preventDefault();
});


function clearCanvas()
{
  context.fillStyle = "#ffffff";
  context.fillRect(0, 0, context.canvas.width, context.canvas.height);
}



function startDraw(x, y)
{
  context.strokeStyle = "#df4b26";
  context.lineJoin = "round";
  context.lineWidth = 5;
  
  context.beginPath();
  context.moveTo(x, y);
}

function addDraw(x, y)
{
  context.lineTo(x, y);
  context.stroke();
}

function endDraw(x, y)
{
}