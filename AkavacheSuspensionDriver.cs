using Akavache;
using ReactiveUI;
using System;
using System.Reactive;

namespace Zephyr;

public class AkavacheSuspensionDriver : ISuspensionDriver {
    private const string AppStateKey = "appState";

    public AkavacheSuspensionDriver() => BlobCache.ApplicationName = "Zephyr";

    public IObservable<Unit> InvalidateState() => BlobCache.UserAccount.InvalidateObject<MainWindowViewModel>(AppStateKey);

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
    public IObservable<object> LoadState() => BlobCache.UserAccount.GetObject<MainWindowViewModel>(AppStateKey);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

    public IObservable<Unit> SaveState(object state) => BlobCache.UserAccount.InsertObject(AppStateKey, (MainWindowViewModel)state);
}

