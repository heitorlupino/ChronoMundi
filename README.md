# ChronoMundi - Museu Interativo Através do Tempo

Um projeto Unity para um museu virtual imersivo, explorando eras históricas através de VR/desktop. Navegue por Pré-História, Idade Média e Futuro Tecnológico, interagindo com artefatos e ouvindo narrações.

## Funcionalidades
- **Navegação por Eras**: Portais no hub levam a ambientes temáticos.
- **Interação com Artefatos**: Pressione E para interagir e ouvir narrações.
- **Interação com Artefatos**: Pressione E para interagir e ouvir narrações. Ao interagir, abre um painel de informações do artefato com título e descrição; feche com a tecla `F` ou com o botão de fechar (opcional). O painel suporta auto-fechar (campo `autoCloseDuration`) e animações configuráveis via `Animator` (gatilhos `openTrigger`/`closeTrigger`, padrões: `Open`/`Close`).
- **Sistema de Progresso**: Complete explorações para desbloquear saídas (UI com ícone dourado).
- **Narrador Inteligente**: Áudio e legendas contextuais.
- **UI Polida**: Menus com gradientes, botões sem warnings de fonte, pause menu aprimorado.
- **Compatibilidade**: Desktop (teclado/mouse) e VR (via XR Toolkit).

## Requisitos
- **Unity**: Versão 2021.3+ (recomendado 2022+).
- **Pacotes**: XR Interaction Toolkit, TextMeshPro, XR Plugin Management.
- **Hardware**: Para VR, headset compatível (Oculus, etc.).

## Instalação e Setup
1. **Clone/Importe o Projeto**:
   - Abra o Unity Hub e importe a pasta do projeto.

2. **Instale Dependências**:
   - Vá para **Window → Package Manager**.
   - Instale: XR Interaction Toolkit, TextMeshPro.
   - Em **Edit → Project Settings → XR Plug-in Management**, ative providers (ex: Oculus).

3. **Configure Assets**:
   - Execute **ChronoMundi → 🎨 Popular Assets + Construir Ambientes** para criar materiais e ambientes.
   - Execute **ChronoMundi → 🏗️ Construir Cenas Completas** para configurar players, UI e componentes.

4. **Adicione Áudios**:
   - Coloque arquivos .wav/.mp3 em `Assets/Projeto/Assets/Audio/`.
   - Atribua nos `InteracleObject` (artefatos) e `TimelineEra` (introduções).

5. **Crie Prefab do Narrador**:
   - Em `Assets/Projeto/Assets/Resources/`, crie GameObject "NarratorSystem" com script `NarratorSystem`.
   - Adicione Canvas filho com TextMeshProUGUI (para `subtitleText`) e painel (para `subtitlePanel`).
   - Salve como prefab "NarratorSystem_Prefab".

6. **Configure Layers e Build**:
   - Adicione layer "Interactable" em **Edit → Project Settings → Tags and Layers**.
   - Vá para **File → Build Settings** e adicione as 5 cenas: MenuScene, MuseuHubScene, PreHistoriaScene, IdadeMediaScene, FuturoTechScene.

## Como Jogar
1. Inicie no MenuScene.
2. Clique "JOGAR" para ir ao hub.
3. Entre nos portais das eras.
4. Explore artefatos (pressione E).
   - Ao interagir com um artefato, um painel exibirá título e descrição; feche com `F` ou o botão de fechar. O comportamento de auto-fechar e animações pode ser configurado no Inspector (`autoCloseDuration`, `panelAnimator`).
5. Complete todos para ativar a saída brilhante.
6. Retorne ao hub e repita.

## Desenvolvimento
- **Scripts Principais**:
  - `ChronoMundiAssetLinker.cs`: Constrói ambientes.
  - `ChronomundiSceneBuilder.cs`: Configura cenas completas.
  - `PlayerInteraction.cs`: Gerencia interações.
  - `NarratorSystem.cs`: Narração.
   - `ArtifactInfoPanel.cs`: Gerencia o painel de informações de artefatos. Expõe `Show(string name, string description)` e `Hide()`, campo `closeKey` (padrão `F`), `autoCloseDuration`, `closeButton` (opcional) e `panelAnimator` com gatilhos configuráveis.
- **Menus Unity**:
  - **🎨 Popular Assets + Construir Ambientes**: Recria materiais/ambientes.
  - **🏗️ Construir Cenas Completas**: Reconfigura cenas.
  - **✅ Validar Projeto**: Relatório de erros/avisos.

## Troubleshooting
- **Warnings no Console**:
  - Script faltando (XR): Remova componentes "(Missing Script)" em objetos XR.
  - Caractere fonte: Mude ▶ para > em textos TextMeshPro.
- **Interações não funcionam**: Verifique layer "Interactable".
- **Narrador mudo**: Atribua AudioClips e crie prefab.
- **Saída não ativa**: Complete todos os artefatos da era.

## Créditos
Desenvolvido com Unity. Assets baseados no VR Template.

## Licença
[Adicione licença se aplicável]