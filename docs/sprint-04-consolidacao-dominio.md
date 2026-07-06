# Sprint 04 - Consolidacao do Dominio

## Objetivo

Consolidar o dominio atual do Renova apos as fundacoes de multi-tenant, seguranca/permissoes e pessoas, preparando o modelo para evoluir com `Person` como fonte central futura de dados pessoais.

Esta etapa preserva compatibilidade com controllers, views e migrations existentes. Nenhuma propriedade foi removida ou renomeada, nenhuma tela foi criada, nenhum controller foi criado e nenhuma migration foi gerada.

## Problemas encontrados

- `Student` ja possui `PersonId` e `Person`, mas ainda mantem dados pessoais proprios.
- `FamilyMember` ja pode se relacionar com `Person`, mas ainda mantem dados pessoais proprios.
- `Professional` ja possui `PersonId` e `Person`, mas ainda mantem dados pessoais proprios.
- `Student` possui `int Status` e tambem `StudentStatus? StudentStatus`.
- `Subscription` possui `int Status`, enquanto o dominio ja possui `SubscriptionStatus`.
- `MedicalRecord` e `Subscription` possuem campos de auditoria proprios e nao herdam `BaseEntity` ou `BaseTenantEntity`.
- `FamilyMember` e `Professional` herdam `BaseTenantEntity`, mas ainda nao tinham filtro global de soft delete.
- `FamilyMember` e `Professional` mantem `Ignore(Tenant)` nas configuracoes EF. Esse mapeamento foi preservado para evitar mudanca estrutural nesta etapa.

## Decisoes tomadas

- `Person` permanece como a entidade central futura para dados pessoais.
- Campos antigos foram mantidos para preservar compatibilidade.
- Campos duplicados foram marcados como `LEGACY` por comentarios XML.
- `int Status` foi mantido onde ja existe e marcado como `LEGACY` quando ha enum equivalente.
- `MedicalRecord` e `Subscription` nao passaram a herdar `BaseTenantEntity` nesta etapa, pois isso exigiria planejamento de migration para adicionar `TenantId`, `IsDeleted` e possiveis relacionamentos.
- Foram adicionados filtros globais de soft delete em `FamilyMember` e `Professional`, pois ambos ja possuem `IsDeleted` por heranca e isso nao exige alteracao de coluna.

## Campos marcados como LEGACY

### Student

- `FullName`: preferir `Person.FullName`.
- `BirthDate`: preferir `Person.BirthDate`.
- `CPF`: preferir `Person.Cpf`.
- `Phone`: preferir `Person.Phone`, `Person.Mobile` ou contatos.
- `Email`: preferir `Person.Email`.
- `Address`: preferir `Person.Address`.
- `PhotoPath`: preferir `Person.PhotoUrl`.
- `Status`: preferir `StudentStatus`.

### FamilyMember

- `FullName`: preferir `Person.FullName`.
- `Relationship`: preferir `RelationshipType` quando possivel.
- `Phone`: preferir `Person.Phone`, `Person.Mobile` ou contatos.
- `Email`: preferir `Person.Email`.
- `PhotoPath`: preferir `Person.PhotoUrl`.

### Professional

- `FullName`: preferir `Person.FullName`.
- `Email`: preferir `Person.Email`.
- `Phone`: preferir `Person.Phone`, `Person.Mobile` ou contatos.
- `PhotoPath`: preferir `Person.PhotoUrl`.

### Subscription

- `Status`: preferir `SubscriptionStatus` em novos fluxos.

## Configuracoes EF revisadas

- `Student` ja possuia relacao um-para-um com `Person` e filtro `!IsDeleted`.
- `FamilyMember` mantem relacao com `Student` e relacao opcional com `Person`; recebeu filtro `!IsDeleted`.
- `Professional` mantem relacao um-para-um opcional com `Person`; recebeu filtro `!IsDeleted`.
- `Employee`, `Volunteer` e `Admission` ja tinham relacao com `Tenant` e filtro `!IsDeleted`.
- `MedicalRecord` e `Subscription` continuam sem filtro de soft delete porque ainda nao possuem `IsDeleted`.

## Proximos passos antes de novas migrations

- Definir a estrategia final de centralizacao dos dados pessoais em `Person`.
- Mapear quais telas e services ainda leem ou gravam campos legados diretamente.
- Definir se `Student.PersonId`, `FamilyMember.PersonId` e `Professional.PersonId` devem se tornar obrigatorios em uma etapa futura.
- Unificar status em novos fluxos usando `StudentStatus` e `SubscriptionStatus`.
- Decidir se `MedicalRecord` e `Subscription` devem herdar `BaseTenantEntity`.
- Planejar migrations somente depois de validar impacto em dados existentes, controllers, views e seed inicial.

## Recomendacao para migracao futura dos dados duplicados para Person

1. Criar migration especifica para backfill de `People` a partir de `Students`, `FamilyMembers` e `Professionals` que ainda nao tenham `PersonId`.
2. Preencher `PersonId` nas entidades legadas apos criar os registros em `People`.
3. Ajustar services e telas para gravarem primeiro em `Person` e manterem campos legados apenas como espelho temporario.
4. Adicionar validacoes para evitar divergencia entre `Person` e campos legados durante o periodo de transicao.
5. Somente apos uma janela segura de compatibilidade, avaliar remocao dos campos legados em uma sprint propria.
