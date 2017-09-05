﻿using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.Daemon;

namespace JetBrains.ReSharper.Plugins.FSharp.Daemon.Cs.Highlightings
{
  public abstract class ErrorOrWarningHighlightingBase : IHighlighting
  {
    private readonly string myMessage;
    private readonly DocumentRange myRange;

    protected ErrorOrWarningHighlightingBase([NotNull] string message, DocumentRange range)
    {
      myMessage = message;
      myRange = range;
    }

    public string ToolTip => myMessage;
    public string ErrorStripeToolTip => myMessage;
    public bool IsValid() => myRange.IsValid();
    public DocumentRange CalculateRange() => myRange;
  }
}