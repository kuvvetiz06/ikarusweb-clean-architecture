// src/framework.js

// küçük ref koleksiyonu: <div data-ref="grid"></div>
function collectRefs(root) {
    const map = {};
    root.querySelectorAll("[data-ref]").forEach(el => {
        map[el.getAttribute("data-ref")] = el;
    });
    return map;
}

export class Component {
    constructor(props = {}, ctx = {}) {
        this.props = props;
        this.ctx = ctx;       // app-level servisler vs.
        this.state = {};
        this._mounted = false;
        this._container = null;
        this._firstRenderDone = false;
        this.$refs = {};
        // (blazor: constructor)
    }

    // ---- lifecycle hook'ları ----
    onInit() { }             // (blazor: OnInitialized/Async, biz sync/await serbest)
    onParametersSet() { }    // (blazor: OnParametersSet)
    render() { return ""; } // string HTML döndür (basitlik için)
    onAfterRender(firstRender) { } // (blazor: OnAfterRender/Async)
    dispose() { }            // (blazor: IDisposable)

    // ---- state yönetimi ----
    setState(patch) {
        this.state = { ...this.state, ...patch };
        this._scheduleRender();
    }

    // ---- internal ----
    _mount(container) {
        this._container = container;
        this._mounted = true;

        // init + first parameter set
        this.onInit();
        this.onParametersSet();

        this._doRender(true);
    }

    _updateProps(newProps) {
        this.props = Object.freeze({ ...this.props, ...newProps });
        this.onParametersSet();
        this._doRender(false);
    }

    _scheduleRender() {
        // microtask (tek frame’de bir kez render)
        if (this._rAf) return;
        this._rAf = Promise.resolve().then(() => {
            this._rAf = null;
            if (this._mounted) this._doRender(false);
        });
    }

    _doRender(isFirst) {
        const html = this.render();
        // basit diff: tüm içeriği değiştir (küçük framework için yeterli)
        this._container.innerHTML = html;
        this.$refs = collectRefs(this._container);

        const firstRender = !this._firstRenderDone;
        this._firstRenderDone = true;
        // DOM hazır → after render
        this.onAfterRender(firstRender);
    }

    _unmount() {
        if (!this._mounted) return;
        this.dispose();
        this._mounted = false;
        this._container.innerHTML = "";
        this.$refs = {};
    }
}

// uygulama düzeyi basit renderer
export function mount(component, container) {
    component._mount(container);
    return () => component._unmount();
}
