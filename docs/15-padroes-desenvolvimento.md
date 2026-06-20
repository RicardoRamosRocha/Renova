# Padrões de Desenvolvimento

## Convenções de Código

### Classes

Utilizar PascalCase.

Exemplo:

```csharp
public class Student
{
}
```

### Propriedades

Utilizar PascalCase.

```csharp
public string FullName { get; set; }
```

### Variáveis

Utilizar camelCase.

```csharp
var student = new Student();
```

### Interfaces

Utilizar prefixo I.

```csharp
public interface IStudentRepository
{
}
```

## Estrutura da Solução

```text
Renova.Web
Renova.API
Renova.Application
Renova.Domain
Renova.Infrastructure
```

## Commits

Padrão:

```text
feat:
fix:
refactor:
docs:
test:
chore:
```

Exemplos:

```text
feat: adicionar cadastro de alunos

fix: corrigir validação de CPF

docs: atualizar documentação do CRM
```

## Branches

```text
main
develop
feature/*
hotfix/*
```

## Banco de Dados

* Entity Framework Core
* Migrations obrigatórias
* Não alterar banco manualmente em produção

## Segurança

* Nunca armazenar senhas em texto puro.
* Utilizar ASP.NET Identity.
* Utilizar HTTPS.
* Registrar auditoria de operações críticas.
