import {
  AuthService,
  Component,
  HTTP_INTERCEPTORS,
  HttpErrorResponse,
  Injectable,
  Router,
  RouterOutlet,
  bootstrapApplication,
  catchError,
  inject,
  provideBrowserGlobalErrorListeners,
  provideHttpClient,
  provideRouter,
  setClassMetadata,
  throwError,
  withComponentInputBinding,
  withInterceptorsFromDi,
  ɵsetClassDebugInfo,
  ɵɵdefineComponent,
  ɵɵdefineInjectable,
  ɵɵelement
} from "./chunk-Z45UCW6N.js";

// src/app/services/auth.interceptor.ts
var AuthInterceptor = class _AuthInterceptor {
  auth = inject(AuthService);
  intercept(req, next) {
    let headers = req.headers.set("Cache-Control", "no-cache, no-store, must-revalidate").set("Pragma", "no-cache").set("Expires", "0");
    const token = this.auth.token();
    if (token && !req.url.endsWith("/api/auth/login")) {
      headers = headers.set("Authorization", `Bearer ${token}`);
    }
    const authReq = req.clone({ headers });
    return next.handle(authReq).pipe(catchError((error) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        this.auth.logout();
      }
      return throwError(() => error);
    }));
  }
  static \u0275fac = function AuthInterceptor_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _AuthInterceptor)();
  };
  static \u0275prov = /* @__PURE__ */ \u0275\u0275defineInjectable({ token: _AuthInterceptor, factory: _AuthInterceptor.\u0275fac });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(AuthInterceptor, [{
    type: Injectable
  }], null, null);
})();

// src/app/services/auth.guard.ts
var authGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  if (auth.isAuthenticated()) {
    return true;
  }
  return router.createUrlTree(["/login"]);
};
var guestGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);
  return auth.isAuthenticated() ? router.createUrlTree(["/"]) : true;
};

// src/app/app.routes.ts
var appRoutes = [
  {
    path: "login",
    loadComponent: () => import("./chunk-DVICY3EQ.js").then((m) => m.LoginComponent),
    canActivate: [guestGuard]
  },
  {
    path: "",
    loadComponent: () => import("./chunk-ADBAE55B.js").then((m) => m.ChatComponent),
    canActivate: [authGuard]
  },
  { path: "**", redirectTo: "" }
];

// src/app/app.config.ts
var appConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(appRoutes, withComponentInputBinding()),
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthInterceptor,
      multi: true
    }
  ]
};

// src/app/app.ts
var AppComponent = class _AppComponent {
  static \u0275fac = function AppComponent_Factory(__ngFactoryType__) {
    return new (__ngFactoryType__ || _AppComponent)();
  };
  static \u0275cmp = /* @__PURE__ */ \u0275\u0275defineComponent({ type: _AppComponent, selectors: [["app-root"]], decls: 1, vars: 0, template: function AppComponent_Template(rf, ctx) {
    if (rf & 1) {
      \u0275\u0275element(0, "router-outlet");
    }
  }, dependencies: [RouterOutlet], styles: ["\n/*# sourceMappingURL=app.css.map */"] });
};
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && setClassMetadata(AppComponent, [{
    type: Component,
    args: [{ selector: "app-root", standalone: true, imports: [RouterOutlet], template: "<router-outlet></router-outlet>\n", styles: ["/* src/app/app.css */\n/*# sourceMappingURL=app.css.map */\n"] }]
  }], null, null);
})();
(() => {
  (typeof ngDevMode === "undefined" || ngDevMode) && \u0275setClassDebugInfo(AppComponent, { className: "AppComponent", filePath: "src/app/app.ts", lineNumber: 11 });
})();

// src/main.ts
bootstrapApplication(AppComponent, appConfig).catch((err) => console.error(err));
//# sourceMappingURL=main.js.map
