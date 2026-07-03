# Deploy do Renova no Render com PostgreSQL Neon

Este guia publica o projeto MVC `Renova.Web` no Render usando Docker e banco PostgreSQL Neon.

## Pré-requisitos

- Repositório com o `Dockerfile` na raiz.
- Banco PostgreSQL criado no Neon.
- Connection string do Neon no formato Npgsql.

## Variáveis de ambiente no Render

Configure no serviço Web do Render:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
ConnectionStrings__DefaultConnection=Host=<host-neon>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true
```

O ASP.NET Core lê `ConnectionStrings__DefaultConnection` automaticamente como `ConnectionStrings:DefaultConnection`.

## Configuração do serviço no Render

1. Crie um novo **Web Service**.
2. Conecte o repositório do Renova.
3. Selecione **Docker** como runtime.
4. Use o `Dockerfile` da raiz.
5. Configure a porta como `8080`, se solicitado.
6. Adicione as variáveis de ambiente acima.
7. Faça o deploy.

## Build local da imagem

Na raiz do repositório:

```bash
docker build -t renova-web .
```

Para executar localmente com uma connection string local:

```bash
docker run --rm -p 8080:8080 \
  -e ConnectionStrings__DefaultConnection="Host=host.docker.internal;Port=5432;Database=renova_db;Username=postgres;Password=<senha-local>" \
  renova-web
```

No Windows PowerShell:

```powershell
docker run --rm -p 8080:8080 `
  -e "ConnectionStrings__DefaultConnection=Host=host.docker.internal;Port=5432;Database=renova_db;Username=postgres;Password=<senha-local>" `
  renova-web
```

## Aplicar migrations no Neon

Opção recomendada para demonstração: aplicar migrations a partir da máquina local apontando para o Neon.

PowerShell:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=<host-neon>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true"
dotnet ef database update --project src/Renova.Infrastructure/Renova.Infrastructure.csproj --startup-project src/Renova.Web/Renova.Web.csproj
```

Bash:

```bash
export ConnectionStrings__DefaultConnection="Host=<host-neon>;Port=5432;Database=<database>;Username=<user>;Password=<password>;SSL Mode=Require;Trust Server Certificate=true"
dotnet ef database update --project src/Renova.Infrastructure/Renova.Infrastructure.csproj --startup-project src/Renova.Web/Renova.Web.csproj
```

Se o comando `dotnet ef` não existir, instale a ferramenta:

```bash
dotnet tool install --global dotnet-ef
```

## Desenvolvimento local

O arquivo `src/Renova.Web/appsettings.json` não deve conter senha real. Para desenvolvimento local, use uma destas opções:

- variável de ambiente `ConnectionStrings__DefaultConnection`;
- `src/Renova.Web/appsettings.Development.json`, que está no `.gitignore`;
- user secrets, se preferir.

Exemplo de `appsettings.Development.json` local:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=renova_db;Username=postgres;Password=<senha-local>"
  }
}
```

## Validação

Antes do deploy:

```bash
dotnet build
docker build -t renova-web .
```
