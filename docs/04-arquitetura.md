# Arquitetura

## Padrão Arquitetural

O Renova utilizará Clean Architecture, separando o sistema em camadas independentes e bem definidas.

## Estrutura da Solução

```text
src/
├── Renova.Web
├── Renova.API
├── Renova.Application
├── Renova.Domain
└── Renova.Infrastructure
```

## Renova.Domain

Responsável pelas entidades principais e regras centrais de negócio.

Exemplos:

* Student
* FamilyMember
* Professional
* MedicalRecord
* Appointment
* Course
* Payment

## Renova.Application

Responsável pelos casos de uso, interfaces, DTOs e serviços de aplicação.

## Renova.Infrastructure

Responsável pelo acesso ao banco de dados, integrações externas, pagamentos, e-mail e serviços técnicos.

## Renova.Web

Interface administrativa desenvolvida com Blazor Server.

## Renova.API

API REST usada para integrações externas e futuro aplicativo Flutter.

## Banco de Dados

Banco principal recomendado:

* PostgreSQL

## Autenticação

Será utilizado ASP.NET Core Identity para login, controle de usuários, perfis e permissões.
