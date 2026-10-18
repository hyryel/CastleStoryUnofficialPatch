namespace Brix.Utils;

public delegate TReturn Caller<TTarget, TReturn>(TTarget target, object[] args);
