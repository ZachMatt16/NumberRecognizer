using System.Drawing;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using NumberRecognizer;

namespace Blazor.Components.Pages;

public partial class NumberPredictor : ComponentBase
{
     // canvas reference
    private ElementReference CanvasRef;

    // drawing state (WPF port)
    private bool _isDrawing = false;
    private bool _isErasing = false;
    private double BrushSize = 18;
    private double PreviousX, PreviousY;

    // recognizer / file path
    private string FinalNumber = "";
    private int TrainIterations = 10000;

    // inject JS
    [Inject] IJSRuntime JS { get; set; }

    //----------------------------------------------------------
    //  MOUSE EVENTS  (direct WPF port)
    //----------------------------------------------------------
    public async Task OnMouseDown(MouseEventArgs e)
    {
        if (e.Buttons == 1) _isDrawing = true;     // left click
        if (e.Buttons == 2) _isErasing = true;     // right click

        PreviousX = e.OffsetX;
        PreviousY = e.OffsetY;

        await DrawPoint(e.OffsetX, e.OffsetY);
    }

    public Task OnMouseUp(MouseEventArgs e)
    {
        _isDrawing = false;
        _isErasing = false;
        return Task.CompletedTask;
    }

    public async Task OnMouseMove(MouseEventArgs e)
    {
        if (!_isDrawing && !_isErasing) return;

        await DrawPoint(e.OffsetX, e.OffsetY);
        PreviousX = e.OffsetX;
        PreviousY = e.OffsetY;
    }

    public async Task DrawPoint(double x, double y)
    {
        if (_isDrawing)
        {
            await JS.InvokeVoidAsync("canvas.drawCircle",
                CanvasRef, x, y, BrushSize, "white");
        }
        else if (_isErasing)
        {
            await JS.InvokeVoidAsync("canvas.drawCircle",
                CanvasRef, x, y, BrushSize * 5, "black");
        }
    }

    //----------------------------------------------------------
    //  BUTTONS (port from WPF)
    //----------------------------------------------------------
    public async Task ClearCanvas()
    {
        await JS.InvokeVoidAsync("canvas.clear", CanvasRef);
    }

    public async Task SaveFile()
    {
        // calls JS to save PNG
        await JS.InvokeVoidAsync("canvas.savePng", CanvasRef, "CurrentNumber.png");
    }

    public void RecognizeNumber()
    {
        FinalNumber = "TODO: call nr.PredictNumber()";
    }

    public void TrainModel()
    {
        // plug your recognizer here again
        // nr.TrainModelWithMNIST(TrainIterations);
    }

    public void TrainSingle(int n)
    {
        FinalNumber = $"Train digit {n}";
        // FinalNumber = "" + nr.TrainModel(n);
    }

    public Task SuppressContextMenu(MouseEventArgs e)
    {
        // prevents right-click menu from appearing
        return JS.InvokeVoidAsync("canvas.disableContext").AsTask();
    }
}