# Sprint 4.5 - Auditoria da Arquitetura do Dominio

## Objetivo

Consolidar a arquitetura do dominio antes da Sprint 5 CRM, revisando heranca, multi-tenant, soft delete, status e relacionamentos com `Person`.

Esta sprint nao cria migrations, controllers ou views. Mudancas estruturais que exigem colunas novas, chaves estrangeiras novas ou remocao de propriedades foram apenas mapeadas como pendencias futuras.

## Entidades auditadas

- `Admission`
- `MedicalRecord`
- `MedicalEvolution`
- `Appointment`
- `Payment`
- `Subscription`
- `TenantSubscription`
- `Course`
- `CourseModule`
- `Lesson`
- `Certificate`
- `Employee`
- `Volunteer`
- `Document`
- `Contact`
- `EmergencyContact`
- `Address`

## Heranca

### Herdam BaseTenantEntity

- `Admission`
- `TenantSubscription`
- `Employee`
- `Volunteer`
- `Document`
- `Contact`
- `EmergencyContact`
- `Address`

Essas entidades possuem `TenantId`, `Tenant`, `CreatedAt`, `UpdatedAt` e `IsDeleted` por heranca.

### Nao herdam BaseEntity/BaseTenantEntity

- `MedicalRecord`
- `MedicalEvolution`
- `Appointment`
- `Payment`
- `Subscription`
- `Course`
- `CourseModule`
- `Lesson`
- `Certificate`

Algumas dessas entidades possuem campos equivalentes manuais, como `Id`, `CreatedAt`, `UpdatedAt` ou `IsDeleted`, mas ainda nao seguem o padrao completo de base.

## Multi-tenant

### Consistente hoje

- `Admission`, `TenantSubscription`, `Employee`, `Volunteer`, `Document`, `Contact`, `EmergencyContact` e `Address` possuem `TenantId` por `BaseTenantEntity`.
- `Admission`, `TenantSubscription`, `Employee`, `Volunteer`, `Document`, `Contact`, `EmergencyContact` e `Address` possuem relacionamento EF com `Tenant`.
- `Employee` e `Volunteer` estao corretamente relacionados com `Person` por `PersonId` opcional e relacao um-para-um.

### Inconsistencias mapeadas

- `MedicalRecord`, `MedicalEvolution`, `Appointment`, `Payment`, `Subscription` e `Certificate` sao operacionais e vinculadas a `Student`, mas nao possuem `TenantId` direto.
- `Course`, `CourseModule` e `Lesson` ainda funcionam como catalogo EAD global. Antes de alterar heranca, e preciso decidir se o catalogo sera global ou tenant-scoped.
- `MedicalRecord` e `Subscription` ja tinham TODOs indicando avaliacao futura para `BaseTenantEntity`.

## Soft delete

### Ja consistente

As entidades abaixo possuem `IsDeleted` e `HasQueryFilter`:

- `Admission`
- `TenantSubscription`
- `Employee`
- `Volunteer`
- `Document`
- `Contact`
- `EmergencyContact`
- `Address`

### Ajuste realizado

- `MedicalEvolution` ja possuia `IsDeleted`, mas nao possuia filtro global. Foi adicionado `HasQueryFilter(medicalEvolution => !medicalEvolution.IsDeleted)`.

### Pendencias

- `MedicalRecord`, `Appointment`, `Payment`, `Subscription`, `Course`, `CourseModule`, `Lesson` e `Certificate` nao possuem soft delete padronizado.
- Alterar essas entidades para `BaseEntity` ou `BaseTenantEntity` exige migration e deve ser feito em sprint propria.

## Status

- `Admission` usa `AdmissionStatus`, consistente com enum.
- `TenantSubscription` usa `SubscriptionStatus`, consistente com enum.
- `Subscription.Status` ainda e `int` e ja esta marcado como `LEGACY`; novos fluxos devem preferir `SubscriptionStatus`.
- `Appointment.Status` e `Payment.Status` continuam como `int`. Nao ha enum equivalente no dominio neste momento, entao nao foi feita troca ou marcacao como substituivel.

## Person

- `Employee` possui `PersonId` opcional e relacao um-para-um com `Person`.
- `Volunteer` possui `PersonId` opcional e relacao um-para-um com `Person`.
- A relacao com `Tenant` e soft delete de `Employee` e `Volunteer` estao configurados.
- Nao foram alterados formularios ou fluxos de escrita dessas entidades nesta sprint.

## Ajustes realizados

- Adicionado filtro global de soft delete em `MedicalEvolutionConfiguration`.
- Adicionados comentarios TODO nas entidades sem heranca padronizada para registrar que qualquer mudanca de `BaseEntity`/`BaseTenantEntity` exige migration planejada:
  - `MedicalEvolution`
  - `Appointment`
  - `Payment`
  - `Course`
  - `CourseModule`
  - `Lesson`
  - `Certificate`

## Decisoes tomadas

- Nao alterar heranca nesta sprint para evitar migrations e risco em tabelas existentes.
- Nao criar enums novos para `Appointment.Status` ou `Payment.Status` sem revisar contratos de API, DTOs e telas.
- Nao alterar formulários, layout, autenticacao ou autorizacao.
- Manter `Course`, `CourseModule` e `Lesson` como globais ate decisao explicita sobre multi-tenant no EAD.
- Liberar apenas ajustes seguros que nao alteram schema.

## Pendencias que exigem migration futura

- Avaliar `BaseTenantEntity` para `MedicalRecord`, `MedicalEvolution`, `Appointment`, `Payment`, `Subscription` e `Certificate`.
- Decidir se `Course`, `CourseModule` e `Lesson` serao globais ou tenant-scoped.
- Adicionar `TenantId` direto em entidades operacionais se a regra de isolamento por tenant exigir consultas sem atravessar `Student`.
- Padronizar soft delete em entidades que hoje nao possuem `IsDeleted`.
- Criar enums formais para `Appointment.Status` e `Payment.Status`, se as regras de negocio estabilizarem.
- Migrar `Subscription.Status` de `int` para enum em uma etapa planejada.

## Recomendacao para Sprint 5 CRM

O dominio esta suficientemente consolidado para iniciar a Sprint 5 CRM, desde que a Sprint 5 nao dependa de mudancas estruturais nas entidades operacionais sem migration.

Para a Sprint 5, recomenda-se:

- Usar `Person` e propriedades `Display*` para dados pessoais.
- Preservar campos legados enquanto a migration de backfill nao for planejada.
- Evitar novos acoplamentos diretos a `int Status` quando ja houver enum disponivel.
- Manter consultas tenant-aware usando as entidades que ja possuem `TenantId` ou navegando por `Student` quando necessario.
- Planejar uma sprint tecnica futura para migrations de `TenantId`, soft delete e enums operacionais.
