# Casos de Uso

## UC001 — Cadastrar Aluno

Ator principal: Administrador ou Atendente.

Descrição: Permite cadastrar um novo aluno no sistema.

Fluxo principal:

1. Usuário acessa o cadastro de alunos.
2. Preenche os dados obrigatórios.
3. Salva o cadastro.
4. Sistema registra o aluno como ativo.

## UC002 — Cadastrar Familiar

Ator principal: Administrador ou Atendente.

Descrição: Permite cadastrar familiares vinculados a um aluno.

Fluxo principal:

1. Usuário acessa o perfil do aluno.
2. Adiciona um familiar.
3. Define o grau de parentesco.
4. Define se o familiar terá acesso ao portal.

## UC003 — Registrar Atendimento

Ator principal: Profissional ou Atendente.

Descrição: Permite registrar um atendimento realizado com o aluno.

Fluxo principal:

1. Usuário seleciona o aluno.
2. Informa data, profissional e descrição.
3. Salva o atendimento.
4. Sistema adiciona o registro ao histórico.

## UC004 — Registrar Evolução

Ator principal: Profissional.

Descrição: Permite registrar evolução do aluno no prontuário.

Fluxo principal:

1. Profissional acessa o prontuário.
2. Registra a evolução.
3. Sistema salva o registro com data, hora e responsável.

## UC005 — Criar Curso

Ator principal: Administrador ou Professor.

Descrição: Permite criar cursos na plataforma EAD.

Fluxo principal:

1. Usuário acessa a área de cursos.
2. Cria um novo curso.
3. Adiciona módulos.
4. Adiciona aulas.

## UC006 — Assistir Aula

Ator principal: Aluno.

Descrição: Permite ao aluno assistir aulas disponíveis.

Fluxo principal:

1. Aluno acessa o portal.
2. Seleciona um curso.
3. Abre uma aula.
4. Sistema registra progresso.

## UC007 — Realizar Pagamento

Ator principal: Aluno ou Familiar.

Descrição: Permite realizar pagamento de mensalidade ou assinatura.

Fluxo principal:

1. Usuário acessa a cobrança.
2. Escolhe o método de pagamento.
3. Gateway processa a cobrança.
4. Sistema confirma pagamento via webhook.
5. Acesso é liberado automaticamente.
