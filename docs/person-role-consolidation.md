# Person e Papeis - Consolidacao

## Decisao arquitetural

`Person` e a fonte principal futura para dados pessoais. Entidades como `Student`, `FamilyMember` e `Professional` representam papeis ou contextos de uso dessa pessoa dentro do Renova.

Nesta etapa, a compatibilidade foi preservada: propriedades legadas continuam existindo, formularios atuais continuam gravando nos campos antigos e nenhuma migration foi criada.

## Papeis consolidados

- `Student`: papel de acolhido, com dados clinicos, operacionais, status e vinculos de acompanhamento.
- `FamilyMember`: papel de familiar ou responsavel vinculado ao acolhido.
- `Professional`: papel de profissional tecnico ou clinico.

Todos podem usar `Person` como origem preferencial para dados pessoais quando houver `PersonId`.

## Campos legados mantidos

### Student

- `FullName`
- `BirthDate`
- `CPF`
- `Phone`
- `Email`
- `Address`
- `PhotoPath`

### FamilyMember

- `FullName`
- `Relationship`
- `Phone`
- `Email`
- `PhotoPath`

### Professional

- `FullName`
- `Email`
- `Phone`
- `PhotoPath`

Esses campos continuam disponiveis para compatibilidade com controllers, views, DTOs, formularios e migrations existentes.

## Propriedades Display*

As entidades de papel agora podem expor propriedades calculadas que priorizam `Person` e usam campos legados como fallback.

### Student

- `DisplayName`
- `DisplayCpf`
- `DisplayEmail`
- `DisplayPhone`
- `DisplayBirthDate`
- `DisplayPhotoUrl`

### FamilyMember

- `DisplayName`
- `DisplayEmail`
- `DisplayPhone`
- `DisplayPhotoUrl`

### Professional

- `DisplayName`
- `DisplayEmail`
- `DisplayPhone`
- `DisplayPhotoUrl`

Essas propriedades nao sao mapeadas no EF e devem ser usadas preferencialmente em telas, listagens, detalhes e respostas de leitura.

## Configuracao EF

`FamilyMemberConfiguration` e `ProfessionalConfiguration` ignoram explicitamente as propriedades calculadas.

As relacoes com `Person` foram mantidas:

- `FamilyMember` possui relacao opcional com `Person`.
- `Professional` possui relacao opcional um-para-um com `Person`.

Os filtros de soft delete continuam ativos com `!IsDeleted`.

## Cuidados para novas telas MVC

- Usar `Display*` para exibicao de dados pessoais quando o model for `Student`, `FamilyMember` ou `Professional`.
- Incluir `Person` nas consultas de listagem/detalhe quando a tela precisar exibir os dados centralizados.
- Nao trocar bindings de `Create` e `Edit` para `Person` sem revisar validacoes, view models e estrategia de sincronizacao.
- Evitar usar propriedades `Display*` diretamente em queries EF, pois elas sao calculadas no dominio e nao mapeadas.
- Manter DTOs externos estaveis enquanto consumidores ainda dependem dos nomes antigos.

## Plano futuro de migracao e backfill

1. Identificar registros de `Students`, `FamilyMembers` e `Professionals` sem `PersonId`.
2. Criar registros em `People` a partir dos campos legados.
3. Preencher `PersonId` nos papeis correspondentes.
4. Atualizar fluxos de criacao e edicao para gravarem dados pessoais em `Person`.
5. Manter sincronizacao temporaria entre `Person` e campos legados.
6. Adicionar verificacoes para detectar divergencias durante o periodo de transicao.
7. Planejar remocao ou desuso definitivo dos campos legados apenas em uma sprint propria, com migration e validacao de dados.
