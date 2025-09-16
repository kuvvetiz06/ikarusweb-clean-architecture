# IKARUSWEB – Architecture (Clean + CQRS + Multi-Tenant + BFF)

UI (ASP.NET Core MVC) ── YARP/BFF (server-side proxy & composition)
   │         └─ Session: access_token, UI Cookie (.ikarusweb.auth)
   │
   └────────> API (ASP.NET Core Web API)
                ├─ Application (CQRS: MediatR, FluentValidation, AutoMapper)
                ├─ Domain (Entities, Value Objects, Rules)
                └─ Infrastructure (EF Core, MSSQL, Identity, Interceptors)
                
**Katman Sorumlulukları**
- Domain: Saf kurallar, Entity/VO, davranış, audit/timestamp `BaseEntity`.
- Application: CQRS, DTO/Mapping, Validations, Result, Business use-case.
- Infrastructure: EF Core, DbContext, Configurations, Repos, Identity, TokenService.
- API: Endpoints, Filters, ProblemDetails, Swagger/Scalar.
- UI/BFF: MVC + YARP, server-side auth/refresh, server-side composition.
**Referanslar**
- UI → (proxy) → API
- API → Application → Domain, Infrastructure
- Application ↔ Infrastructure (DI ile)  (Application EF’e doğrudan referans vermez)

# Auth / BFF / Refresh Tasarımı

**Durum**
- UI `[Authorize]` için **CookieAuthentication** kullanır (.ikarusweb.auth).
- **Access token** kısa ömürlü ve **Session**'da tutulur; YARP her /api çağrısında Authorization header ekler.
- **Refresh token** uzun ömürlü **HttpOnly Cookie** (Path=/api/auth, SameSite=Lax, Secure=SameAsRequest).
- **Refresh** server-side: BFF 401 görünce `/api/auth/refresh` çağırır, başarılıysa Session+UI Cookie’yi uzatır ve isteği **tek kez** retry eder.

**Akış**
1) Login → API `Set-Cookie: refresh_token=...`, body: `{ data: { accessToken, expiresAt } }`
2) BFF LoginTransformer → Session: `access_token`, Session: `refresh_token` (opsiyonel yedek), UI: SignIn cookie (Expires = accessToken.exp)
3) Normal /api istek → BFF header'a `Bearer` enjekte
4) 401 → BFF `TryRefresh` → başarılı → Session+Cookie güncelle → orijinali retry
5) Refresh başarısız → BFF SignOut + 401 → UI login

**Güvenlik**
- Refresh cookie HttpOnly+SameSite=Lax, JS erişemez.
- TokenService: Issuer/Audience/Key ve exp kontrolü.
- Multi-tenant: JWT claim `"tenant_id"`, API tarafında filtre Interceptor/QueryFilter.

# Kod Konvansiyonları

**DTO & Result**
- `Result<T> { bool Succeeded; string? Message; T? Data }`
- Hata zarfı: `ApiErrorEnvelope { Problem, Errors[] }` + field bazlı hatalar.

**Adlandırma**
- Komut: `CreateXCommand`, `UpdateXCommand`
- Sorgu: `GetXByIdQuery`, `GetXListQuery`
- Profil: `XProfile`
- EF Config: `XConfiguration`
- Validations: `CreateXValidator` …

**Çeviri (i18n)**
- UI: `IStringLocalizer<SharedResource>`, `Resources/Views.[culture].resx`
- API mesajları lokalize edilmeyecek; UI’da gösterim lokalize edilecek.

**PR & Branch**
- Branch: `PR-00xx/short-slug`
- PR başlığı: `PR-00xx: Topic`
- Commit: Conventional Commits (`feat:`, `fix:`, `refactor:`, `docs:`)

# PR Süreci (Atomik Scaffold & Mini PR'lar)
- Her PR **tek amaç**: küçük, okunabilir, geri alma kolay.
- Şablon (aşağıdaki PR template ile uyumlu).

## Özet
<!-- PR-00xx: Kısa açıklama -->

## Değişiklik Türü
- [ ] feat
- [ ] fix
- [ ] refactor
- [ ] docs
- [ ] chore

## İçerik
- [ ] Katman(lar): UI / API / Application / Infrastructure / Domain
- [ ] Migration var mı? [ ] evet [ ] hayır
- [ ] Breaking change var mı? [ ] evet [ ] hayır

## Test / Doğrulama
- [ ] Manuel senaryolar (adım adım)
- [ ] Postman/HTTP dosyası linki

## İlgili Doküman
- Closes #ISSUE
- Updated: docs/Auth-BFF-Refresh.md, docs/Architecture.md

name: Feature
description: Yeni özellik veya iyileştirme
labels: ["feature"]
body:
  - type: input
    id: scope
    attributes:
      label: Scope (tek amaç)
      description: Bu issue tek bir hedefe odaklanmalı
  - type: textarea
    id: details
    attributes:
      label: Detaylar
  - type: checkboxes
    id: tasks
    attributes:
      label: Tasks
      options:
        - label: UI
        - label: API
        - label: Application
        - label: Infrastructure
        - label: Domain
        - label: Docs
