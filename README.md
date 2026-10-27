# Vendinha

Sistema de controle de clientes e dívidas de uma vendinha (as "contas penduradas"), feito em C# com .NET 10 e banco de dados SQLite.

## Funcionalidades

- Cadastro, consulta, atualização e exclusão de clientes
  - Nome completo, CPF e data de nascimento obrigatórios
  - CPF validado pelos dígitos verificadores e único no sistema
  - E-mail opcional, validado quando informado
  - Idade calculada a partir da data de nascimento
- Listagem de clientes
  - Ordenada do que mais deve para o que menos deve
  - Paginação de 10 em 10
  - Busca por parte do nome
  - Mostra a idade e o total de dívidas em aberto de cada cliente
- Dívidas por cliente
  - Valor, situação (paga ou não), data de criação e data de pagamento
  - Listagem das dívidas de um cliente
  - Marcar uma dívida como paga
  - Regra: cada cliente só pode ter uma dívida em aberto

## Tecnologias

- .NET 10 / C#
- ASP.NET Core (Minimal APIs)
- SQLite com Microsoft.Data.Sqlite
- Swagger (Swashbuckle) para documentação e testes da API

## Estrutura do projeto

```
Vendinha.slnx
database/
  schema.sql              Script de criação do banco
Vendinha/                 Biblioteca com as regras do sistema
  Models/                 Cliente e Divida
  Validations/            Validação de CPF
  Services/               ClienteService e DividaService
  Data/                   Conexão com o SQLite
Vendinha.Api/             API web
  Endpoints/              Rotas de clientes e de dívidas
  Requests/               Formato dos pedidos
  Responses/              Formato das respostas de erro
  Program.cs              Configuração da API
```

## Como executar

Requisito: [.NET SDK 10](https://dotnet.microsoft.com/download).

```bash
git clone https://github.com/luiz692/vendinha.git
cd vendinha
dotnet run --project Vendinha.Api
```

Depois abra **http://localhost:5080** no navegador. A página leva direto para o Swagger, onde dá para ver e testar todas as rotas.

Na primeira execução, o arquivo do banco (`vendinha.db`) é criado sozinho dentro da pasta `Vendinha.Api`, usando o script `database/schema.sql`.

## Banco de dados

O script de criação está em [`database/schema.sql`](database/schema.sql). Ele cria duas tabelas:

| Tabela | Colunas |
| --- | --- |
| `clientes` | id, nome, cpf, data_nascimento, email |
| `dividas` | id, cliente_id, valor, paga, data_criacao, data_pagamento |

Regras garantidas pelo próprio banco:

- `cpf` é único
- `valor` precisa ser maior que zero
- uma dívida precisa pertencer a um cliente existente
- excluir um cliente exclui as dívidas dele
- índice único parcial em `dividas (cliente_id) WHERE paga = 0`, que impede duas dívidas em aberto para o mesmo cliente

Para criar o banco manualmente com o SQLite instalado:

```bash
sqlite3 vendinha.db < database/schema.sql
```

## Rotas da API

| Método | Rota | Descrição |
| --- | --- | --- |
| GET | `/clientes?pagina=1&busca=maria` | Lista os clientes, 10 por página, do que mais deve para o que menos deve |
| GET | `/clientes/{id}` | Mostra um cliente |
| POST | `/clientes` | Cadastra um cliente |
| PUT | `/clientes/{id}` | Atualiza um cliente |
| DELETE | `/clientes/{id}` | Exclui um cliente e as dívidas dele |
| GET | `/clientes/{clienteId}/dividas` | Lista as dívidas de um cliente |
| POST | `/clientes/{clienteId}/dividas` | Lança uma dívida nova para o cliente |
| PUT | `/dividas/{id}/pagar` | Marca uma dívida como paga |

### Exemplos

Cadastrar um cliente (`POST /clientes`):

```json
{
  "nome": "Maria Souza",
  "cpf": "529.982.247-25",
  "dataNascimento": "2000-05-10",
  "email": "maria@email.com"
}
```

Resposta (`201 Created`):

```json
{
  "id": 1,
  "nome": "Maria Souza",
  "cpf": "52998224725",
  "dataNascimento": "2000-05-10T00:00:00",
  "email": "maria@email.com",
  "totalDivida": 0,
  "idade": 26
}
```

Lançar uma dívida (`POST /clientes/1/dividas`):

```json
{
  "valor": 150.75
}
```

Quando algum dado está errado, a API responde `400 Bad Request` com a lista de problemas:

```json
{
  "erros": [
    "CPF inválido",
    "E-mail inválido"
  ]
}
```

Outros códigos de resposta:

- `404 Not Found`: cliente ou dívida não encontrados
- `204 No Content`: cliente excluído
