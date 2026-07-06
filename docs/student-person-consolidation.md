# Student e Person - Consolidacao

## Decisao arquitetural

`Person` passa a ser a fonte principal para dados pessoais em novos fluxos. `Student` continua existindo como entidade de contexto do acolhido, mantendo dados clinicos, operacionais e propriedades legadas necessarias para compatibilidade com controllers, views, DTOs e migrations existentes.

Nesta etapa nao houve remocao ou renomeacao de propriedades, nao houve migration nova e nenhum fluxo de criacao/edicao foi reescrito em massa.

## Campos legados mantidos em Student

Os campos abaixo continuam em `Student` para preservar compatibilidade:

- `FullName`
- `BirthDate`
- `CPF`
- `Phone`
- `Email`
- `Address`
- `PhotoPath`

Eles devem ser tratados como legado em novos fluxos. Enquanto a migracao definitiva nao ocorrer, podem continuar recebendo dados por formularios existentes.

## Novas propriedades calculadas

`Student` agora expoe propriedades de leitura que priorizam `Person` e usam os campos legados como fallback:

- `DisplayName`: `Person.FullName` ou `Student.FullName`.
- `DisplayCpf`: `Person.Cpf` ou `Student.CPF`.
- `DisplayEmail`: `Person.Email` ou `Student.Email`.
- `DisplayPhone`: `Person.Phone` ou `Student.Phone`.
- `DisplayBirthDate`: `Person.BirthDate` ou `Student.BirthDate`.
- `DisplayPhotoUrl`: `Person.PhotoUrl` ou `Student.PhotoPath`.

Essas propriedades sao marcadas com `[NotMapped]` e tambem ignoradas em `StudentConfiguration`, evitando qualquer alteracao de schema.

## Como novas telas devem consumir dados pessoais

Novas telas, listagens, detalhes e respostas de leitura devem preferir as propriedades `Display*` quando estiverem trabalhando com `Student`.

Quando a tela ou servico precisar editar dados pessoais, a evolucao recomendada e gravar em `Person` e manter os campos legados apenas como espelho temporario, ate que exista uma migration e uma estrategia de backfill.

Formularios existentes de `Create` e `Edit` nao foram alterados nesta etapa para evitar quebra de model binding, validacoes ou comportamento esperado.

## Configuracao EF

`Student` mantem a relacao opcional um-para-um com `Person` por `PersonId`.

O filtro global de soft delete em `Student` continua ativo com `!IsDeleted`.

As propriedades calculadas `Display*` nao sao mapeadas pelo EF.

## Plano futuro para migracao

1. Mapear todos os estudantes sem `PersonId`.
2. Criar registros em `People` a partir dos dados legados de `Students`.
3. Preencher `Students.PersonId` para os registros migrados.
4. Atualizar fluxos de criacao e edicao para gravarem dados pessoais em `Person`.
5. Manter sincronizacao temporaria com campos legados enquanto houver telas antigas.
6. Criar validacoes ou jobs de auditoria para detectar divergencias entre `Person` e campos legados.
7. Somente em sprint futura, apos estabilizacao, avaliar remocao dos campos legados.

## Cuidados de compatibilidade

- Nao trocar atribuicoes de formulario diretamente para `Person` sem revisar binding e validacoes.
- Nao usar propriedades `Display*` dentro de queries EF sem materializar ou sem expressao translatavel.
- Carregar `Person` com `Include` em listagens/detalhes que precisam exibir dados centralizados.
- Nao criar migration ate que o plano de backfill esteja validado.
- Manter DTOs e rotas atuais estaveis enquanto consumidores externos ainda dependem dos nomes antigos.
