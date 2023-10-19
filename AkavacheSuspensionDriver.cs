using Akavache;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Zephyr;

public class AkavacheSuspensionDriver : ISuspensionDriver {
    private const string AppStateKey = "appState";

    public AkavacheSuspensionDriver() {
#if DEBUG
        BlobCache.ApplicationName = "ZephyrDebug";
#else
        BlobCache.ApplicationName = "Zephyr";
#endif
    }

    public IObservable<Unit> InvalidateState() => Observable.Start(() => { });

#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
    public IObservable<object> LoadState() => BlobCache.Secure.GetObject<MainWindowViewModel>(AppStateKey);
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.

    public IObservable<Unit> SaveState(object state) => BlobCache.Secure.InsertObject(AppStateKey, (MainWindowViewModel)state);
}

