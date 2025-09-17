
```
██████╗ ███████╗██████╗  █████╗ ████████╗██╗███████╗ █████╗ 
██╔══██╗██╔════╝██╔══██╗██╔══██╗╚══██╔══╝██║██╔════╝██╔══██╗
██████╔╝█████╗  ██████╔╝███████║   ██║   ██║███████╗███████║
██╔═══╝ ██╔══╝  ██╔══██╗██╔══██║   ██║   ██║╚════██║██╔══██║
██║     ███████╗██║  ██║██║  ██║   ██║   ██║███████║██║  ██║
╚═╝     ╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝   ╚═╝   ╚═╝╚══════╝╚═╝  ╚═╝
```

![Language](https://img.shields.io/badge/language-C%23-512BD4?logo=csharp)  
![License](https://img.shields.io/badge/license-Proprietary-red)  
![Version](https://img.shields.io/badge/version-1.0.0-blue)  
![Platforms](https://img.shields.io/badge/platforms-Windows%20%7C%20Linux%20%7C%20macOS-lightgrey)

---

# Rebatiza — Rename & Replace Tool

---

## 🇧🇷 Português

**Rebatiza** é uma ferramenta de console que permite **renomear arquivos e pastas** e **substituir texto dentro de arquivos** em todo um repositório ou projeto.

É ideal para migrações de código, refatoração de *namespaces*, troca de nomes de produtos/marcas ou qualquer tarefa de substituição em larga escala.

### ✨ Funcionalidades
- ✅ **Substitui conteúdo** de arquivos de texto (busca literal)  
- ✅ **Renomeia arquivos e pastas**  
- ✅ **Escolha de idioma**: Português ou Inglês  
- ✅ **Dry-run (simulação)**: mostra o que seria feito sem alterar nada  
- ✅ **Barra de progresso** e **log colorido**  
- ✅ **Tolera erros**: continua mesmo que alguns itens falhem  
- ✅ **Resumo final** com estatísticas e detalhes de erros  

---

## 🇺🇸 English

**Rebatiza** is a console tool that helps you **rename files and folders** and **replace text inside files** across an entire repository or project.

It is designed for codebase migrations, namespace refactoring, product/brand renaming, or any large-scale replacement task.

### ✨ Features
- ✅ **Content replacement** in text files (literal string match)  
- ✅ **File and folder renaming**  
- ✅ **Language selection**: English or Portuguese  
- ✅ **Dry-run mode** (*simulation*): preview changes without applying them  
- ✅ **Progress bar** and **colored logs**  
- ✅ **Error tolerance**: keeps running even if some items fail  
- ✅ **Final summary** with stats and error details  

---

## 🚀 Uso / Usage

### 🇧🇷 Passo a passo
1. **Escolher idioma**  
   ```
   Select language / Selecione o idioma [1=English, 2=Português]:
   ```
2. **Escolher modo de execução**  
   ```
   Modo de execução [1=Ao vivo (altera), 2=Dry-run (simula)]:
   ```
   - Ao vivo = aplica as alterações  
   - Dry-run = apenas simula, não altera nada  
3. **Informar a pasta raiz**  
4. **Informar texto antigo e novo**  
5. Acompanhar a **barra de progresso**  
6. Conferir o **resumo final**  

### 🇺🇸 Step by step
1. **Choose language**  
   ```
   Select language / Selecione o idioma [1=English, 2=Português]:
   ```
2. **Choose run mode**  
   ```
   Run mode [1=Live (apply), 2=Dry-run (simulate)]:
   ```
   - Live = changes are applied  
   - Dry-run = no changes, only simulation  
3. **Enter root folder path**  
4. **Enter old text and replacement text**  
5. Watch the **progress bar**  
6. Review the **final summary**  

---

## 🔧 Opções / Options

- `-n` ou `--dry-run` → inicia diretamente em modo simulação / starts directly in dry-run mode  

---

## 📊 Exemplo de saída / Example output

```
Select language / Selecione o idioma [1=English, 2=Português]: 2
Modo de execução [1=Ao vivo (altera), 2=Dry-run (simula)]: 2
[DRY-RUN] Modo simulação ativado — nenhum arquivo será alterado.

Pasta raiz (ex.: C:\Projetos\MeuApp): C:\Projetos\MeuApp
Texto a substituir: OldName
Novo texto: NewName

Serão processados 54 arquivo(s) e 12 pasta(s).
Iniciando...

[████████░░░░░░░░░░░░░░░░░░]  45%  (30/66)
[DRY-RUN] Atualizaria conteúdo: Program.cs
[DRY-RUN] Renomearia arquivo: OldNameService.cs → NewNameService.cs
[DRY-RUN] Renomearia pasta: OldNameLib → NewNameLib
...

✅ Concluído (simulação). Nenhuma alteração foi aplicada.
Modo: Dry-run (simulação)
Arquivos com conteúdo alterado: 12
Arquivos renomeados: 5
Pastas renomeadas: 2
```

---

## ⚙️ Filtros padrão / Default filters

**🇧🇷 Pastas ignoradas**: `.git`, `bin`, `obj`, `.vs`, `.idea`, `.vscode`  
**🇺🇸 Ignored folders**: `.git`, `bin`, `obj`, `.vs`, `.idea`, `.vscode`  

**🇧🇷 Extensões binárias ignoradas**: `.dll`, `.exe`, `.png`, `.jpg`, `.jpeg`, `.gif`, `.zip`, `.ico`, `.pdb`, `.db`, `.woff`, `.woff2`, `.eot`, `.pdf`, `.7z`, `.tar`, `.gz`  
**🇺🇸 Ignored binary extensions**: `.dll`, `.exe`, `.png`, `.jpg`, `.jpeg`, `.gif`, `.zip`, `.ico`, `.pdb`, `.db`, `.woff`, `.woff2`, `.eot`, `.pdf`, `.7z`, `.tar`, `.gz`  

---

## 🛡 Erros / Errors

- **🇧🇷** Cada item é processado em bloco isolado (`try/catch`). Se falhar, o erro é logado e o processo continua.  
- **🇺🇸** Each item is processed inside `try/catch`. If it fails, the error is logged and processing continues.  

---

## ✅ Boas práticas / Best practices

- 🇧🇷 Rode primeiro em **dry-run** para revisar  
- 🇺🇸 Run **dry-run** first to preview  

- 🇧🇷 Faça **commit** no Git antes de rodar em modo ao vivo  
- 🇺🇸 **Commit changes** before running live mode  

- 🇧🇷 Feche IDEs e processos que possam bloquear arquivos  
- 🇺🇸 Close IDEs/build processes that might lock files  

---

## 📌 Limitações / Limitations

- 🇧🇷 Substituição literal (case-sensitive).  
- 🇺🇸 Literal replacement (case-sensitive).  

- 🇧🇷 Assume UTF-8.  
- 🇺🇸 Assumes UTF-8.  

- 🇧🇷 Não usa regex.  
- 🇺🇸 No regex support.  

- 🇧🇷 Carrega arquivo inteiro em memória.  
- 🇺🇸 Loads full file into memory.  

- 🇧🇷 Não sobrescreve nomes já existentes.  
- 🇺🇸 Does not overwrite existing names.  

---

## 🔮 Futuro / Future ideas

- Filtros glob (`--include`, `--exclude`)  
- Exportar relatório `.txt` ou `.json`  
- Regex opcional  
- Backup automático  
- Execução paralela  
- Detecção de encoding  

---

## 📜 Licença / License

**PROPRIETÁRIA** — Copyright (c) 2025 Rodrigo de Freitas Oliveira  
**Todos os direitos reservados.** Uso, modificação e distribuição proibidos sem autorização.

Para uso comercial ou distribuição, contate o autor.  

---

## ⚖️ Aviso Legal / Legal Notice

**🇧🇷 Português:**
- Este software é propriedade exclusiva de Rodrigo de Freitas Oliveira
- Uso apenas para fins pessoais e não comerciais
- Proibida cópia, modificação, distribuição ou venda sem autorização
- Violações podem resultar em ações legais

**🇺🇸 English:**
- This software is exclusive property of Rodrigo de Freitas Oliveira
- Use only for personal and non-commercial purposes
- Copying, modification, distribution or sale prohibited without authorization
- Violations may result in legal action

---

## 🔖 Nome / Name

- **Rebatiza** (recomendado / recommended)  
- 🇧🇷 Alternativas: Renomeia+, TrocaNome, NomeShift  
- 🇺🇸 Alternatives: RenameX, SwapShift, RefactorIt, Find&Swap  
