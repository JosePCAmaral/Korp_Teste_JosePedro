# Korp_Teste_JosePedro

Sistema de emissão de Notas Fiscais, desenvolvido como desafio técnico do processo seletivo da Korp ERP. A aplicação é composta por um front-end em Angular e um back-end organizado em dois microsserviços em C#/.NET, com persistência real em banco de dados, tratamento de falhas entre serviços e três funcionalidades extras (IA local, idempotência e tratamento de concorrência).

## Arquitetura

```
Korp_Teste_JosePedro/
├── Estoque.Api/         # Microsserviço de controle de produtos e saldos
├── Faturamento.Api/     # Microsserviço de gestão de notas fiscais
└── frontend/            # Aplicação Angular (SPA)
```

- **Estoque.Api**: CRUD de produtos, controle de saldo, e o único serviço que fala com a IA local (Ollama).
- **Faturamento.Api**: CRUD de notas fiscais e orquestração da emissão/impressão, chamando o Estoque.Api via HTTP para validar e baixar saldo.
- **frontend**: Angular standalone (sem NgModules), consumindo as duas APIs diretamente.

Os dois serviços têm bancos de dados próprios e independentes (`KorpEstoqueDb` e `KorpFaturamentoDb`) — nenhum dos dois acessa a tabela do outro diretamente, só via chamada HTTP, respeitando o isolamento de dados esperado de uma arquitetura de microsserviços.

## Tecnologias utilizadas

**Back-end**
- C# / .NET 8 — ASP.NET Core Web API
- Entity Framework Core (Code First + Migrations)
- SQL Server LocalDB
- `HttpClientFactory` para comunicação entre os microsserviços
- Ollama (execução local de LLM) para a funcionalidade de IA

**Front-end**
- Angular 17+ (componentes standalone)
- RxJS (Observables no consumo das APIs via `HttpClient`)
- Angular Material (componentes visuais)
- Reactive Forms

## Funcionalidades

### Obrigatórias
- Cadastro de Produtos (código, descrição, saldo)
- Cadastro de Notas Fiscais (numeração sequencial, status Aberta/Fechada, múltiplos itens)
- Impressão de Notas Fiscais: indicador de carregamento, baixa de saldo, bloqueio de reimpressão de notas não abertas
- Arquitetura de microsserviços (Estoque + Faturamento)
- Tratamento de falha: se o Estoque.Api fica indisponível durante a impressão, o Faturamento.Api responde com erro amigável (HTTP 503) e a nota permanece "Aberta" — assim que o serviço volta, a impressão funciona normalmente, sem inconsistência de dados
- Persistência real em banco de dados (SQL Server LocalDB, um banco por serviço)

### Opcionais implementadas
- **Tratamento de concorrência**: coluna de controle de versão (`RowVersion`/concorrência otimista) no Produto. Duas requisições de baixa de saldo disputando o mesmo produto (ex.: saldo 1, duas notas simultâneas) resultam em uma bem-sucedida e outra rejeitada com erro claro, sem jamais deixar o saldo negativo.
- **Uso de Inteligência Artificial**: botão "Sugerir descrição com IA" no cadastro de produto. O Estoque.Api chama um modelo LLM rodando localmente via [Ollama](https://ollama.com) (`llama3.2:1b`), gerando uma sugestão de nome de produto a partir do código informado — sem depender de internet ou de chave de API paga.
- **Idempotência**: o endpoint de impressão de nota (`POST /api/notasfiscais/{id}/imprimir`) aceita um header opcional `Idempotency-Key`. Repetir a mesma chamada com a mesma chave devolve a resposta já processada anteriormente, sem repetir a baixa de saldo.

## Como executar

### Pré-requisitos
- .NET 8 SDK
- SQL Server LocalDB (instalado junto com o Visual Studio, workload "ASP.NET e desenvolvimento Web")
- Node.js 18+ e Angular CLI (`npm install -g @angular/cli`)
- [Ollama](https://ollama.com) instalado, com o modelo `llama3.2:1b` baixado (`ollama pull llama3.2:1b`) — necessário apenas para a funcionalidade de sugestão de descrição via IA; o restante do sistema funciona normalmente sem ele.

### Backend
1. Abra `Korp_Teste_JosePedro.sln` no Visual Studio.
2. Restaure os pacotes NuGet (feito automaticamente ao abrir a solution).
3. Aplique as migrations em cada projeto (Console do Gerenciador de Pacotes, alternando o "Default project"):
   ```
   Update-Database   # com Estoque.Api selecionado
   Update-Database   # com Faturamento.Api selecionado
   ```
4. Configure "Vários Projetos de Inicialização" nas propriedades da solução, com `Estoque.Api` e `Faturamento.Api` como "Start".
5. Rode com F5. As APIs sobem com Swagger habilitado (ex.: `https://localhost:7085/swagger` e `https://localhost:7247/swagger`, as portas exatas ficam em `Properties/launchSettings.json` de cada projeto).

### Frontend
```bash
cd frontend
npm install
ng serve
```
Acesse `http://localhost:4200`.

> As URLs das APIs usadas pelo Angular ficam em `frontend/src/environments/environment.development.ts` — ajuste se as portas dos serviços forem diferentes na sua máquina.

## Endpoints principais

**Estoque.Api**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/produtos` | Lista produtos |
| GET | `/api/produtos/{id}` | Detalhe de um produto |
| POST | `/api/produtos` | Cadastra produto |
| PUT | `/api/produtos/{id}` | Atualiza produto |
| POST | `/api/produtos/{id}/baixa` | Baixa saldo (uso interno, chamado pelo Faturamento) |
| GET | `/api/produtos/sugerir-descricao?codigo=` | Sugestão de descrição via IA local |

**Faturamento.Api**
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/notasfiscais` | Lista notas fiscais |
| GET | `/api/notasfiscais/{id}` | Detalhe de uma nota |
| POST | `/api/notasfiscais` | Cria nota fiscal (status inicial "Aberta") |
| POST | `/api/notasfiscais/{id}/imprimir` | Emite a nota: baixa saldo no Estoque e fecha a nota (aceita header opcional `Idempotency-Key`) |

## Entrega

- Repositório: este mesmo repositório público
- Vídeo de demonstração: *(link a adicionar)*
- Documento de detalhamento técnico: *(link a adicionar)*

## Autor

José Pedro

