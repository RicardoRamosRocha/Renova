# Renova Experience — Foundation visual

## Contrato

```text
Orizon.UI
  -> Renova Foundation
    -> composições específicas do domínio
      -> telas
```

`Orizon.UI` é a fonte dos componentes visuais genéricos e dos tokens que já
existirem. A Renova Foundation contém somente aliases semânticos, escala para
novas composições e regras de integração. Ela não substitui `orizon-card`,
`orizon-button`, `orizon-table`, `orizon-field` ou outros componentes do
Orizon.

## Versão e pipeline atual

- Pacote: `Orizon.UI 1.2.0`, em `src/Renova.Web/Renova.Web.csproj`.
- Áreas administrativas usam `Areas/Admin/Views/Shared/_AdminLayout.cshtml`.
- `_AdminLayout` carrega, nesta ordem:
  1. `_content/Orizon.UI/css/orizon.css`;
  2. `wwwroot/css/renova-foundation.css`;
  3. `wwwroot/css/dashboard-v2.css`;
  4. `wwwroot/css/admin-premium.css`.
- `wwwroot/css/admin-premium.css` importa a árvore CSS legada: tokens,
  layout, sidebar, topbar, buttons, cards, tables, forms, badges, avatars,
  profile, timeline, dashboard, CRM, EAD, Medical, premium-components,
  utilities e responsive.
- `@section Styles` das views entra depois do CSS do layout.
- Estilos `<style>` dentro das views e estilos inline entram por último.
- O `Views/Shared/_LandingLayout.cshtml` possui pipeline separado e carrega
  Orizon UI, `app.css` e scripts da landing page. A foundation administrativa
  não foi aplicada à landing page neste bloco.

O bundle legado foi mantido intacto para preservar os módulos atuais durante a
migração progressiva.

## Escala para novas composições

Spacing oficial da foundation: `1, 2, 3, 4, 5, 6, 8, 10, 12`, baseado em
incrementos de `0.25rem`: `4, 8, 12, 16, 20, 24, 32, 40, 48px`.

Novos componentes devem preferir os tokens equivalentes do Orizon. Os aliases
`--renova-*` existem para estabilizar o contrato do Renova sem obrigar as
telas legadas a uma migração imediata.

## Contrato dos padrões futuros

- PageHeader: eyebrow, título, descrição, breadcrumb, status e ações; usar
  Orizon quando houver suporte suficiente.
- Botões: `orizon-button` como padrão; variantes oficiais primary, neutral,
  outline, ghost e danger.
- Status: aparência baseada em `orizon-badge`; significado permanece no
  domínio Renova.
- Cards: `orizon-card` como base para surface, border, radius, shadow e
  padding.
- Tabelas: `orizon-table` + `orizon-table-container`; decidir por conteúdo
  entre overflow horizontal, redução de colunas ou composição mobile.
- Formulários: `orizon-field`, `orizon-input`, `orizon-select`,
  `orizon-textarea`, `orizon-form-grid` e `orizon-form-actions`.
- Empty state: reutilizar Orizon se suficiente; não criar outra implementação
  paralela.
- Paginação: convergir para `orizon-pagination`.
- Feedback: `orizon-alert` para mensagens; confirmações acessíveis são
  necessidade futura. Os `confirm()` atuais não foram alterados.

## Matriz de migração

| Legado | Padrão oficial | Destino | Estratégia |
|---|---|---|---|
| `admin-page-header` | PageHeader | Orizon/UI composition | Migrar por tela |
| `crm-page-header` | PageHeader | Orizon/UI composition | Migrar por tela |
| `admin-form-header` | PageHeader + FormSection | Orizon/UI composition | Migrar por tela |
| `patient-hero` | DetailHeader de domínio | Renova | Avaliar após PageHeader |
| `demo-hero` | PageHeader | Orizon/UI composition | Migrar por tela |
| `rd-hero` | DashboardHeader | Orizon UI / R2 | Evoluir componente compartilhado |
| `medical-page-header` | PageHeader | Orizon/UI composition | Migrar por tela |
| `crm-status-badge` | `orizon-badge` | Orizon UI | Mapear semântica no Renova |
| `renova-status-badge` | `orizon-badge` | Orizon UI | Mapear semântica no Renova |
| `demo-pill` | `orizon-badge` | Orizon UI | Migrar por tela |
| `rd-status` | `orizon-badge` | Orizon UI / R2 | Evoluir se necessário |
| `admin-status-pill` | `orizon-badge` | Orizon UI | Migrar por tela |
| `admin-table` | `orizon-table` | Orizon UI | Migrar após estratégia mobile |
| `demo-table` | `orizon-table` | Orizon UI | Migrar por tela |
| `rd-table` | `orizon-table` / DataTable | Orizon UI / R2 | Evoluir para dashboard |
| `rn-pagination` | `orizon-pagination` | Orizon UI | Migrar por tela |
| `admin-pagination` | `orizon-pagination` | Orizon UI | Migrar por tela |
| `_Pagination.cshtml` | Pagination composition | Renova ou Orizon | Decidir contrato de parâmetros |
| `_PremiumEmptyState` | EmptyState | Orizon UI | Reutilizar/evoluir antes de duplicar |
| `crm-empty-state` | EmptyState | Orizon UI | Migrar por tela |
| `admin-empty-state` | EmptyState | Orizon UI | Migrar por tela |
| `rn-premium-empty` | EmptyState | Orizon UI | Migrar por tela |
| `patient-card` | DetailSection | Orizon UI + Renova | Compor sem domínio no componente base |
| `patient-profile-grid` | Detail layout | Renova composition | Preservar domínio |
| `renova-action-btn` | Button/icon-button | Orizon UI | Corrigir accessible name na migração |

## Fora do escopo deste bloco

Não foram redesenhados Acolhidos, Admissions, Famílias, Appointments,
Profissionais ou qualquer Dashboard. Não foram alterados controllers,
ViewModels, entidades, migrations, banco, autorização, rotas, queries ou o
repositório Orizon UI.
