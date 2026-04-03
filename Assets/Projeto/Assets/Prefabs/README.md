# Prefabs do ChronoMundi

Esta pasta guarda os prefabs base do projeto.

## Como gerar/atualizar
1. Abra a Unity com este projeto.
2. Menu superior: **ChronoMundi → Prefabs → Recriar Prefabs Base**.
3. O gerador cria/atualiza:
   - `GameManager_Prefab.prefab`
   - `NarratorSystem_Prefab.prefab`
   - `LoadingScreen_Prefab.prefab`

Além desta pasta, os mesmos arquivos também são copiados para `Assets/Resources/` para funcionar automaticamente com `SceneBootstrapper`.

## Observações
- O gerador é idempotente (pode rodar várias vezes).
- Se você customizar os prefabs manualmente, mantenha os nomes com sufixo `_Prefab`.
- Para fluxos de equipe, prefira editar o script gerador para manter padrão entre máquinas.
