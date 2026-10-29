# Matriz de Compatibilidade e Diagnóstico de Dependências

## 1. Bibliotecas e Assemblies Encontrados

| Assembly | Status Original | Requisito iOS | Ação Recomendada |
|---|---|---|---|
| `Assembly-CSharp.dll` | Mono / CIL (1.341 tipos) | IL2CPP (AOT) | Decompilado com sucesso (809 arquivos `.cs`). Inspecionar e resolver incompatibilidades de AOT. |
| `Assembly-CSharp-firstpass.dll` | Tayx Graphy | IL2CPP | Mantido para testes de performance/FPS ou desativado em release. |
| `Rewired_Core.dll` | Multiplataforma | IL2CPP | Compatível com Unity, requer mapeamento de controles iOS. |
| `Rewired_Windows.dll` | Específico de Windows | Incompatível | **Substituir / Desativar.** |
| `Rewired_DirectInput.dll` | DLL Nativa x86_64 | Incompatível | **Remover.** |
| `Steamworks.net.dll` | Wrapper Steam | Incompatível | **Remover / Stub.** |
| `steam_api64.dll` | DLL Nativa Steam | Incompatível | **Remover.** |
| `Microsoft.Xbox.*` / `XGamingRuntime*.dll` | Xbox GDK | Incompatível | **Remover.** |
| `DOTween.dll` / `DOTweenPro.dll` | Tweening | Compatível | Requer `link.xml` para garantir que métodos com reflection não sejam excluídos pelo Stripping IL2CPP. |
| `EasyDecal.Runtime.dll` | Sistema de Graffiti/Decals | Compatível | Validar shaders no Metal. |
| `Unity.TextMeshPro.dll` | UI Text | Totalmente compatível | Preservar fontes e atlases. |
| `Unity.Timeline.dll` | Cutscenes/Sequences | Totalmente compatível | Preservar sequências e playables. |
| `Unity.AI.Navigation.dll` | NavMesh IA | Totalmente compatível | Preservar dados de NavMesh. |

---

## 2. Adaptação do Código Decompilado

### A. Remoção de Chamadas a Steam
No arquivo descompilado `SteamManager.cs`:
- Desativar a inicialização automática do Steamworks (`SteamAPI.Init()`).
- O sistema de conquistas do BRC já possui uma classe `AchievementsLocal` (conquistas locais sem Steam), permitindo fallback transparente.

### B. Sistema de Entrada (Input)
- Criar controles na tela (Virtual On-Screen Joystick e Botões de Ação para Pulo, Trick, Boost, Grind).
- Interceptar os eventos através de `UserInputHandler.cs`.

### C. Sistema de Salvamento
- Adaptar o sistema de salvamento para gravar em `Application.persistentDataPath` (pasta Sandbox do iOS).
