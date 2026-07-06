# Person e Papeis - Fluxo de Escrita

## Objetivo

Consolidar a escrita de `FamilyMember` e `Professional` com `Person`, seguindo o mesmo padrao aplicado em `Student`.

`Person` passa a ser criado e atualizado automaticamente nos fluxos de criacao e edicao, enquanto os campos legados continuam sendo gravados para manter compatibilidade com controllers, views, DTOs e migrations existentes.

## Fluxo de criacao

### FamilyMember

O fluxo de criacao da API continua recebendo e preenchendo os campos legados:

- `FullName`
- `Email`
- `Phone`
- `PhotoPath`, quando existir em fluxo futuro

Antes de salvar, o familiar chama `SyncPersonFromLegacyFields(...)`.

Se `Person` ainda nao existir, o metodo cria uma nova pessoa, associa `PersonId` e sincroniza os dados equivalentes. O `TenantId` do familiar e herdado do `Student` vinculado.

### Professional

Os fluxos de criacao da API e do MVC continuam preenchendo os campos legados:

- `FullName`
- `Email`
- `Phone`
- `PhotoPath`, quando existir em fluxo futuro

Antes de salvar, o profissional chama `SyncPersonFromLegacyFields(...)`.

Se `Person` ainda nao existir, o metodo cria uma nova pessoa, associa `PersonId` e sincroniza os dados equivalentes.

## Fluxo de edicao

Nos fluxos de edicao, `FamilyMember` e `Professional` sao carregados com `Person`.

Os campos legados continuam sendo atualizados a partir dos requests ou view models existentes. Depois disso, `SyncPersonFromLegacyFields(...)` atualiza o `Person` associado.

Se um registro antigo ainda nao tiver `Person`, o metodo cria uma nova pessoa durante a edicao e associa o novo `PersonId`.

## Sincronizacao temporaria

Durante a fase de transicao, os campos legados permanecem como espelho dos dados pessoais:

- `FamilyMember.FullName`, `Email`, `Phone`, `PhotoPath`
- `Professional.FullName`, `Email`, `Phone`, `PhotoPath`

`Person` recebe os mesmos valores e deve ser tratado como fonte principal futura para novos fluxos.

## Uso futuro de Person

Novas telas e APIs de leitura devem preferir as propriedades `Display*` ou carregar `Person` diretamente quando precisarem de dados pessoais.

Novos fluxos de escrita devem evoluir para gravar primeiro em `Person` e manter os campos legados apenas como compatibilidade temporaria.

## Futura remocao dos campos legados

1. Criar backfill para registros antigos sem `PersonId`.
2. Garantir que todos os fluxos de escrita criem ou atualizem `Person`.
3. Monitorar divergencias entre campos legados e `Person`.
4. Atualizar consumidores internos e externos para usar `Person` ou `Display*`.
5. Planejar a remocao dos campos legados em uma sprint propria, com migration e validacao de dados.

## Cuidados

- Nao alterar formularios visualmente durante esta fase.
- Nao remover propriedades legadas antes da migration de transicao.
- Nao usar `Display*` como origem de queries EF complexas.
- Carregar `Person` nos fluxos de edicao para preservar o registro existente.
- Criar `Person` automaticamente apenas quando o papel ainda nao tiver pessoa associada.
