# Decisão de Arquitetura: Port de Bomb Rush Cyberfunk para iOS

**Data:** 29 de Outubro de 2026  
**Alvo:** iPhone 12 (iOS)  
**Distribuição:** Pacote `.app` para Sideload / TrollStore (sem necessidade de assinatura por certificado de desenvolvedor oficial)

---

## 1. Evidências Encontradas

A auditoria estática realizada em `E:\SteamLibrary\steamapps\common\BombRushCyberfunk` revelou:

- **Versão da Unity:** `2021.3.20f1` (LTS) confirmada via cabeçalho do `globalgamemanagers`.
- **Scripting Backend:** **Mono** (não IL2CPP) no executável de Windows. Confirmado pela presença de `MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll` e ausência de `GameAssembly.dll`.
- **Assemblies Gerenciados:** `Assembly-CSharp.dll` (1.76 MB) contém 1.341 tipos C# (1.069 em `Reptile`), decompilados com sucesso em 809 arquivos `.cs` via `ilspycmd`.
- **Asset Bundles:** 87 arquivos na pasta `StreamingAssets\Assets\` totalizando ~1.56 GB (cenários, malhas combinadas, shaders, animações, músicas e diálogos).
- **Plugins Nativos Windows:** `steam_api64.dll`, `Rewired_DirectInput.dll`, `Microsoft.Xbox.Services.141.GDK.C.Thunks.dll`, `XGamingRuntimeThunks.dll`, `XCurl.dll`.

---

## 2. Comparação das Estratégias

### Estratégia A — Adaptar Projeto Recuperado com AssetRipper (Escolhida)
- **Vantagens:** Preserva hierarquia de prefabs, colisores, animações, configurações de cena, materiais e mapeamento de dependências.
- **Viabilidade:** Alta. Como o jogo usa Mono, os scripts e tipos C# correspondem 1:1 com os metadados dos assets e serialização da Unity.
- **Ações necessárias:**
  1. Extração completa via AssetRipper.
  2. Substituição do código C# pelo descompilado via ILSpy (garantindo legibilidade e manutenção).
  3. Remoção de bibliotecas Windows/Xbox/Steam.
  4. Adaptação do sistema de entrada para touch/iOS.

### Estratégia B — Reconstrução do Zero no Unity
- **Avaliação:** Mantida como fallback caso certas cenas do AssetRipper apresentem corrupção irreversível.

### Estratégia C — Migração de Engine
- **Avaliação:** Descartada. Custo inviável para o escopo do projeto.

---

## 3. Decisão Formal

**Adotada a Estratégia A.**  
O projeto extraído servirá como base do projeto Unity `port/UnityProject`. As alterações de plataforma serão feitas diretamente sobre esta base.
