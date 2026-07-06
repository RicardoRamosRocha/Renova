# Student e Person - Fluxo de Escrita

## Objetivo

Fazer os fluxos de criacao e edicao de `Student` gravarem `Person` como fonte principal futura de dados pessoais, mantendo os campos legados de `Student` sincronizados para compatibilidade.

Nenhuma migration foi criada, nenhuma propriedade foi removida ou renomeada, e as views/formularios atuais permanecem visualmente iguais.

## Fluxo de criacao

O formulario ou request continua preenchendo os campos legados de `Student`:

- `FullName`
- `BirthDate`
- `CPF`
- `Phone`
- `Email`
- `Address`
- `PhotoPath`, quando houver upload no MVC

Apos preencher esses campos, o fluxo chama `Student.SyncPersonFromLegacyFields(...)`.

Esse metodo cria `Person` quando ainda nao existe e copia os dados equivalentes:

- `Student.FullName` -> `Person.FullName`
- `Student.BirthDate` -> `Person.BirthDate`
- `Student.CPF` -> `Person.Cpf`
- `Student.Email` -> `Person.Email`
- `Student.Phone` -> `Person.Phone`
- `Student.PhotoPath` -> `Person.PhotoUrl`

Depois disso, `Student.PersonId` fica associado ao `Person` criado.

## Fluxo de edicao

Os fluxos de edicao carregam `Student` com `Person`.

O formulario/request continua atualizando os campos legados de `Student`, preservando binding e validacoes existentes. Em seguida, `Student.SyncPersonFromLegacyFields(...)` atualiza o `Person` existente.

Se um `Student` antigo ainda nao tiver `Person`, o metodo cria um novo `Person` durante a edicao e associa `Student.PersonId`.

## Sincronizacao temporaria

Durante esta etapa, os campos legados continuam sendo gravados para preservar compatibilidade com controllers, views, DTOs, consultas antigas e migrations ja existentes.

`Person` passa a receber os mesmos dados pessoais a cada criacao ou edicao, funcionando como fonte central para novos fluxos de leitura e futuras telas.

## Estrategia futura para remover campos legados

1. Criar migration de backfill para gerar `People` para estudantes antigos sem `PersonId`.
2. Garantir que todos os fluxos de escrita gravem diretamente em `Person`.
3. Manter os campos legados apenas como espelho durante uma janela de transicao.
4. Adicionar verificacoes para detectar divergencias entre `Student` e `Person`.
5. Atualizar consumidores internos e externos para usar `Display*` ou `Person`.
6. Somente em sprint futura, avaliar remocao dos campos legados com migration propria.

## Cuidados de compatibilidade

- Nao alterar visualmente `Create` e `Edit` sem revisar os view models.
- Nao remover campos legados enquanto controllers, views ou DTOs ainda dependem deles.
- Carregar `Person` em fluxos de edicao para atualizar o registro existente.
- Criar `Person` automaticamente apenas quando o estudante ainda nao tiver um associado.
- Evitar usar propriedades `Display*` como origem de binding; elas devem ser usadas principalmente para leitura.
