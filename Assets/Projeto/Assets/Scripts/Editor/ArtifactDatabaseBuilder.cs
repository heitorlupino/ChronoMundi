#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary>
/// Cria todos os ScriptableObjects de artefatos com os textos históricos completos.
/// Execute UMA VEZ para popular Assets/Projeto/Assets/Data/Artifacts/
///
/// Depois os RoomBuilders leem esses SOs automaticamente.
/// Para atualizar um texto: abra o SO no Inspector e edite diretamente.
///
/// Menu: ChronoMundi → 📚 Criar Database de Artefatos (textos)
/// </summary>
public static class ArtifactDatabaseBuilder
{
    const string OUTPUT_PATH = "Assets/Projeto/Assets/Data/Artifacts";

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/📚 Criar Database de Artefatos (textos)")]
    public static void CreateAll()
    {
        EnsureDirectories();

        // ── Pré-História ─────────────────────────────────────────────────
        Create(new ArtifactDataSO
        {
            artifactID          = "Machado_PreHistoria",
            era                 = EraType.PreHistoria,
            artifactName        = "Machado de Pedra",

            subtitleText        = "Machado de pedra — 2,5 milhões de anos",

            artifactDescription =
                "O machado de pedra lascada é uma das ferramentas mais antigas já criadas pelo ser humano, " +
                "com aproximadamente 2,5 milhões de anos.\n\n" +
                "Fabricado por meio da técnica de lascamento — bater uma pedra contra outra para criar uma " +
                "borda afiada — era usado para cortar carne, quebrar ossos e trabalhar madeira.\n\n" +
                "Representa o primeiro salto tecnológico da humanidade: transformar um recurso natural " +
                "bruto em uma ferramenta com propósito definido. Esse momento marcou a separação " +
                "definitiva entre os hominídeos e os outros primatas.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        Create(new ArtifactDataSO
        {
            artifactID          = "Osso_PreHistoria",
            era                 = EraType.PreHistoria,
            artifactName        = "Osso Numerado",

            subtitleText        = "Os primeiros números da humanidade — 25.000 anos",

            artifactDescription =
                "O Osso de Ishango, encontrado no atual Congo e datado de aproximadamente 25.000 anos, " +
                "é considerado por muitos pesquisadores o primeiro artefato matemático da história.\n\n" +
                "Suas marcas entalhadas em grupos regulares podem representar contagens de ciclos lunares " +
                "ou sequências numéricas com padrão intencional — evidência de pensamento abstrato.\n\n" +
                "Este osso demonstra que a capacidade de abstração matemática surgiu muito antes " +
                "da escrita, das cidades ou de qualquer civilização formal. A matemática não foi " +
                "inventada — foi descoberta por mentes que já enxergavam padrões no mundo natural.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        Create(new ArtifactDataSO
        {
            artifactID          = "Pintura_PreHistoria",
            era                 = EraType.PreHistoria,
            artifactName        = "Pintura Rupestre",

            subtitleText        = "Arte nas pedras — os primeiros registros da humanidade",

            artifactDescription =
                "As pinturas rupestres são expressões artísticas feitas com pigmentos minerais " +
                "como ocre vermelho, carvão e manganês nas paredes de cavernas. As mais antigas " +
                "têm mais de 40.000 anos — anteriores à chegada do ser humano moderno na Europa.\n\n" +
                "Representam animais em movimento, caçadas, mãos em negativo e símbolos geométricos. " +
                "Mais do que simples decoração, eram possivelmente registros sagrados, mapas de caça " +
                "ou formas de comunicação ritual entre membros do grupo.\n\n" +
                "São o primeiro sistema de comunicação visual da humanidade — a origem de toda " +
                "escrita, de toda arte e de toda transmissão cultural registrada.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        // ── Idade Média ───────────────────────────────────────────────────
        Create(new ArtifactDataSO
        {
            artifactID          = "Pergaminho_IdadeMedia",
            era                 = EraType.IdadeMedia,
            artifactName        = "Pergaminho Iluminado",

            subtitleText        = "Manuscrito iluminado — séculos V a XV",

            artifactDescription =
                "Os manuscritos iluminados eram livros produzidos inteiramente à mão por monges " +
                "escribas em scriptoria — oficinas monásticas dedicadas à cópia e decoração de textos.\n\n" +
                "Cada página podia levar semanas de trabalho: as letras capitais eram adornadas com " +
                "folha de ouro genuína, pigmentos raros como lápis-lazúli trazido do Afeganistão " +
                "e ilustrações microscópicas com bordas de criaturas fantásticas.\n\n" +
                "Eram os objetos mais valiosos da Idade Média — uma Bíblia iluminada podia custar " +
                "mais do que uma fazenda inteira. Preservaram textos da Antiguidade Clássica que, " +
                "sem os monges, teriam se perdido para sempre.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        Create(new ArtifactDataSO
        {
            artifactID          = "Escudo_IdadeMedia",
            era                 = EraType.IdadeMedia,
            artifactName        = "Escudo Heráldico",

            subtitleText        = "Brasão de família nobre — identidade e honra em metal",

            artifactDescription =
                "Os brasões heráldicos funcionavam como identidades visuais das famílias nobres " +
                "medievais — um sistema de reconhecimento visual indispensável em tempos de batalha, " +
                "quando rostos ficavam ocultos por capacetes de metal.\n\n" +
                "Cada elemento era codificado com precisão: o vermelho (goles) simbolizava coragem " +
                "e ousadia; o dourado (ouro), nobreza e generosidade; o leão, força e bravura em combate. " +
                "A combinação era única para cada linhagem e protegida por lei.\n\n" +
                "O escudo era passado de pai para filho como relíquia sagrada. Perder ou manchar " +
                "o brasão da família era considerado a maior desonra que um cavaleiro poderia sofrer.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        Create(new ArtifactDataSO
        {
            artifactID          = "Espada_IdadeMedia",
            era                 = EraType.IdadeMedia,
            artifactName        = "Espada de Cavaleiro",

            subtitleText        = "A lâmina do cavaleiro — símbolo de honra e poder medieval",

            artifactDescription =
                "A espada de cavaleiro era muito mais do que uma arma: era o símbolo máximo do " +
                "código da cavalaria e do compromisso do guerreiro com honra, proteção e lealdade.\n\n" +
                "Forjada por ferreiros especializados ao longo de semanas, o aço era dobrado e " +
                "martelado centenas de vezes para equilibrar dureza e flexibilidade. A guarda em " +
                "cruz não era apenas funcional — carregava simbolismo cristão profundo.\n\n" +
                "Cavaleiros recebiam suas espadas em cerimônias formais de investidura, muitas " +
                "vezes com nomes próprios — como Excalibur ou Durandal. Eram transmitidas por " +
                "gerações como símbolo da continuidade da linhagem e de sua honra imortal.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        // ── Futuro Tech ───────────────────────────────────────────────────
        Create(new ArtifactDataSO
        {
            artifactID          = "Holograma_FuturoTech",
            era                 = EraType.FuturoTech,
            artifactName        = "Interface Holográfica",

            subtitleText        = "Projeção holográfica — o futuro das interfaces humano-máquina",

            artifactDescription =
                "A interface holográfica projeta imagens tridimensionais diretamente no espaço " +
                "físico usando lasers e difração de luz coerente, sem necessidade de tela ou superfície.\n\n" +
                "Ao contrário de projeções comuns, os hologramas têm profundidade real: " +
                "o observador pode se mover ao redor da imagem e vê-la de diferentes ângulos. " +
                "Sensores de gesto permitem manipular os dados com as mãos diretamente no ar.\n\n" +
                "Aplicações incluem salas cirúrgicas onde médicos veem órgãos projetados sobre o " +
                "paciente em tempo real, ambientes de colaboração global imersiva e interfaces " +
                "adaptativas que se reconfiguram para cada usuário.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        Create(new ArtifactDataSO
        {
            artifactID          = "Chip_FuturoTech",
            era                 = EraType.FuturoTech,
            artifactName        = "Processador Quântico",

            subtitleText        = "Computação quântica — além dos limites do silício",

            artifactDescription =
                "O processador quântico opera com qubits — unidades de informação que, ao contrário " +
                "dos bits clássicos (0 ou 1), existem em superposição de ambos os estados simultaneamente.\n\n" +
                "Isso permite que um computador quântico explore bilhões de possibilidades em paralelo. " +
                "Um problema de otimização que levaria um supercomputador clássico 10.000 anos " +
                "pode ser resolvido em minutos.\n\n" +
                "Aplicações críticas incluem: simulação molecular para descoberta acelerada de remédios, " +
                "quebra (e criação) de criptografia inquebrável, otimização de cadeias logísticas globais " +
                "e modelagem climática com precisão sem precedentes. Este chip representa " +
                "a maior revolução computacional desde o transistor.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        Create(new ArtifactDataSO
        {
            artifactID          = "Exo_FuturoTech",
            era                 = EraType.FuturoTech,
            artifactName        = "Exoesqueleto Neural",

            subtitleText        = "Exoesqueleto neural — a fusão entre humano e máquina",

            artifactDescription =
                "O exoesqueleto neural é controlado diretamente por sinais elétricos do cérebro, " +
                "capturados por eletrodos não-invasivos posicionados na superfície do crânio.\n\n" +
                "O sistema de IA interpreta padrões de ativação neural em microssegundos e os " +
                "traduz em movimentos mecânicos precisos. Pacientes com lesão medular completa " +
                "voltam a andar. Trabalhadores industriais carregam 200kg sem esforço. " +
                "Cirurgiões operam com tremor zero e precisão sub-milimétrica.\n\n" +
                "Representa a convergência de quatro áreas: neurociência, robótica, inteligência " +
                "artificial e ciência dos materiais. O exoesqueleto não é uma prótese — é uma " +
                "extensão do corpo humano que elimina os limites entre biologia e tecnologia.",

            textAuthor   = "ChronoMundi",
            lastRevision = "2025-05",
        });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("ChronoMundi ✅ Database Criado",
            "9 ScriptableObjects criados em:\n" +
            "Assets/Projeto/Assets/Data/Artifacts/\n\n" +
            "Para editar textos: selecione o SO no Project e edite no Inspector.\n" +
            "Para aplicar à cena: execute o RoomBuilder da era correspondente.\n\n" +
            "SOs criados:\n" +
            "• Machado_PreHistoria\n• Osso_PreHistoria\n• Pintura_PreHistoria\n" +
            "• Pergaminho_IdadeMedia\n• Escudo_IdadeMedia\n• Espada_IdadeMedia\n" +
            "• Holograma_FuturoTech\n• Chip_FuturoTech\n• Exo_FuturoTech",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════

    static void Create(ArtifactDataSO data)
    {
        string path = $"{OUTPUT_PATH}/{data.artifactID}.asset";

        // Se já existe, atualiza apenas os textos sem destruir AudioClip atribuído
        var existing = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(path);
        if (existing != null)
        {
            existing.artifactName        = data.artifactName;
            existing.subtitleText        = data.subtitleText;
            existing.artifactDescription = data.artifactDescription;
            existing.era                 = data.era;
            existing.textAuthor          = data.textAuthor;
            existing.lastRevision        = data.lastRevision;
            EditorUtility.SetDirty(existing);
            Debug.Log($"[ArtifactDB] Atualizado: {data.artifactID}");
            return;
        }

        AssetDatabase.CreateAsset(data, path);
        Debug.Log($"[ArtifactDB] Criado: {data.artifactID}");
    }

    static void EnsureDirectories()
    {
        foreach (var dir in new[] {
            "Assets/Projeto",
            "Assets/Projeto/Assets",
            "Assets/Projeto/Assets/Data",
            "Assets/Projeto/Assets/Data/Artifacts"
        })
        {
            var parts = dir.Split('/');
            string cur = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = cur + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(cur, parts[i]);
                cur = next;
            }
        }
    }

    /// <summary>
    /// Carrega um ArtifactDataSO pelo ID.
    /// Usado pelos RoomBuilders para buscar dados sem path hardcoded.
    /// </summary>
    public static ArtifactDataSO Load(string artifactID)
    {
        string path = $"{OUTPUT_PATH}/{artifactID}.asset";
        var so = AssetDatabase.LoadAssetAtPath<ArtifactDataSO>(path);
        if (so == null)
            Debug.LogError($"[ArtifactDB] SO não encontrado: '{path}'. " +
                           "Execute 'ChronoMundi → 📚 Criar Database de Artefatos' primeiro.");
        return so;
    }
}
#endif
