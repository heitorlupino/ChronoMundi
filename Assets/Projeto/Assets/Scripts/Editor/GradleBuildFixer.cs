#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEditor.XR.Management;

/// <summary>
/// Corrige os 3 erros do Gradle build:
///
///   ERRO 1 — Theme.AppCompat.NoActionBar not found
///     Causa: xrmanifest.androidlib (do XR Management) referencia AppCompat
///            mas a dependência não está no gradle.
///     Fix:   Cria mainTemplate.gradle com androidx.appcompat incluído
///            + desativa XR Management no Android.
///
///   ERRO 2 — minSdkVersion (32) > targetSdkVersion (22)
///     Causa: targetSdkVersion estava em 22 (padrão antigo).
///     Fix:   targetSdkVersion → 33.
///
///   ERRO 3 — StackOverflowError
///     Causa: consequência do erro 1 (manifest corrompido).
///     Fix:   resolvido automaticamente ao corrigir o erro 1.
///
/// Menu: ChronoMundi → 🔨 Corrigir Erros de Build Android
/// </summary>
public static class GradleBuildFixer
{
    const string PLUGINS_DIR   = "Assets/Plugins/Android";
    const string GRADLE_TMPL   = "Assets/Plugins/Android/mainTemplate.gradle";
    const string LAUNCHER_TMPL = "Assets/Plugins/Android/launcherTemplate.gradle";
    const string GRADLE_PROPS  = "Assets/Plugins/Android/gradleTemplate.properties";

    // ════════════════════════════════════════════════════════════════════
    [MenuItem("ChronoMundi/🔨 Corrigir Erros de Build Android")]
    public static void Fix()
    {
        if (!EditorUtility.DisplayDialog("ChronoMundi — Fix Build Android",
            "Corrige os 3 erros do Gradle:\n\n" +
            "1. Theme.AppCompat.NoActionBar not found\n" +
            "   → adiciona appcompat ao mainTemplate.gradle\n\n" +
            "2. minSdkVersion (32) > targetSdkVersion (22)\n" +
            "   → targetSdkVersion = 33\n\n" +
            "3. StackOverflowError no manifest\n" +
            "   → desativa XR Management no Android\n\n" +
            "Continuar?", "Corrigir", "Cancelar"))
            return;

        Prog("Configurando SDK versions...", 0.15f);
        FixSdkVersions();

        Prog("Desativando XR Management no Android...", 0.35f);
        DisableXRManagementAndroid();

        Prog("Criando mainTemplate.gradle com AppCompat...", 0.55f);
        WriteMainTemplate();

        Prog("Criando launcherTemplate.gradle...", 0.70f);
        WriteLauncherTemplate();

        Prog("Criando gradleTemplate.properties...", 0.82f);
        WriteGradleProperties();

        Prog("Salvando...", 0.95f);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.ClearProgressBar();

        EditorUtility.DisplayDialog("ChronoMundi ✅ Build Corrigido",
            "Correções aplicadas!\n\n" +
            "✔ targetSdkVersion → 33\n" +
            "✔ XR Management desativado no Android\n" +
            "✔ mainTemplate.gradle criado\n" +
            "✔ launcherTemplate.gradle criado\n" +
            "✔ gradleTemplate.properties criado\n\n" +
            "Agora tente o Build novamente:\n" +
            "File → Build Settings → Android → Build",
            "OK");
    }

    // ════════════════════════════════════════════════════════════════════
    // FIX 1 — SDK Versions
    // ════════════════════════════════════════════════════════════════════

    static void FixSdkVersions()
    {
        // minSdk 24 (requisito Cardboard) — já estava correto
        if ((int)PlayerSettings.Android.minSdkVersion < 24)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        // targetSdk 33 — era 22, causava o erro
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;

        Debug.Log("[BuildFixer] SDK versions: min=24, target=33");
    }

    // ════════════════════════════════════════════════════════════════════
    // FIX 2 — Desativa XR Management no Android
    // (remove o xrmanifest.androidlib que causa o Theme.AppCompat erro)
    // ════════════════════════════════════════════════════════════════════

    static void DisableXRManagementAndroid()
    {
        var settings = XRGeneralSettingsPerBuildTarget
            .XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);

        if (settings == null)
        {
            Debug.Log("[BuildFixer] XRGeneralSettings Android não encontrado — OK.");
            return;
        }

        var so = new SerializedObject(settings);

        var autoLoad = so.FindProperty("m_AutomaticLoading");
        var autoRun  = so.FindProperty("m_AutomaticRunning");

        if (autoLoad != null) autoLoad.boolValue = false;
        if (autoRun  != null) autoRun.boolValue  = false;

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(settings);

        Debug.Log("[BuildFixer] XR Management Android: AutomaticLoading/Running desativados.");
    }

    // ════════════════════════════════════════════════════════════════════
    // FIX 3 — mainTemplate.gradle com AppCompat
    // ════════════════════════════════════════════════════════════════════

    static void WriteMainTemplate()
    {
        EnsureDir(PLUGINS_DIR);

        // Sempre sobrescreve para garantir que o template está correto
        if (File.Exists(GRADLE_TMPL))
        {
            File.Delete(GRADLE_TMPL);
            Debug.Log("[BuildFixer] mainTemplate.gradle antigo removido.");
        }

        string content = @"// Gerado pelo ChronoMundi GradleBuildFixer
// Corrige: Theme.AppCompat.NoActionBar not found

apply plugin: 'com.android.library'

dependencies {
    implementation fileTree(dir: 'libs', include: ['*.jar'])

    // FIX: AppCompat necessário pelo xrmanifest.androidlib do XR Management
    implementation 'androidx.appcompat:appcompat:1.6.1'
    implementation 'androidx.constraintlayout:constraintlayout:2.1.4'
    **DEPS**
}

android {
    compileSdkVersion **APIVERSION**
    buildToolsVersion '**BUILDTOOLS**'

    compileOptions {
        sourceCompatibility JavaVersion.VERSION_11
        targetCompatibility JavaVersion.VERSION_11
    }

    defaultConfig {
        minSdkVersion **MINSDKVERSION**
        targetSdkVersion **TARGETSDKVERSION**
        ndk {
            abiFilters **ABIFILTERS**
        }
        versionCode **VERSIONCODE**
        versionName '**VERSIONNAME**'
        consumerProguardFiles 'proguard-unity.txt'**USER_PROGUARD**
    }

    lintOptions {
        abortOnError false
    }

    aaptOptions {
        ignoreAssetsPattern = '!.svn:!.git:!.ds_store:!*.scc:.*:!CVS:!thumbs.db:!picasa.ini:!*~'
        noCompress = ['.unity3d', '.ress', '.resource', '.obb'] + unityStreamingAssets.tokenize(', ')
    }
}
";
        File.WriteAllText(GRADLE_TMPL, content);
        Debug.Log("[BuildFixer] mainTemplate.gradle criado com appcompat.");
    }

    // ════════════════════════════════════════════════════════════════════
    // launcherTemplate.gradle — targetSdk explícito
    // ════════════════════════════════════════════════════════════════════

    static void WriteLauncherTemplate()
    {
        EnsureDir(PLUGINS_DIR);

        // Sempre sobrescreve
        if (File.Exists(LAUNCHER_TMPL))
        {
            File.Delete(LAUNCHER_TMPL);
            Debug.Log("[BuildFixer] launcherTemplate.gradle antigo removido.");
        }

        string content = @"// Gerado pelo ChronoMundi GradleBuildFixer
apply plugin: 'com.android.application'

dependencies {
    implementation project(':unityLibrary')
    implementation 'androidx.appcompat:appcompat:1.6.1'
}

android {
    compileSdkVersion **APIVERSION**
    buildToolsVersion '**BUILDTOOLS**'

    compileOptions {
        sourceCompatibility JavaVersion.VERSION_11
        targetCompatibility JavaVersion.VERSION_11
    }

    defaultConfig {
        applicationId '**APPLICATIONID**'
        minSdkVersion **MINSDKVERSION**
        targetSdkVersion **TARGETSDKVERSION**
        versionCode **VERSIONCODE**
        versionName '**VERSIONNAME**'
    }

    lintOptions {
        abortOnError false
    }

    aaptOptions {
        ignoreAssetsPattern = '!.svn:!.git:!.ds_store:!*.scc:.*:!CVS:!thumbs.db:!picasa.ini:!*~'
        noCompress = ['.unity3d', '.ress', '.resource', '.obb'] + unityStreamingAssets.tokenize(', ')
    }

    **SIGNING_CONFIG**

    buildTypes {
        debug {
            minifyEnabled **MINIFY_DEBUG**
            proguardFiles getDefaultProguardFile('proguard-android.txt')
            **SIGNCONFIG**
        }
        release {
            minifyEnabled **MINIFY_RELEASE**
            proguardFiles getDefaultProguardFile('proguard-android.txt')
            **SIGNCONFIG**
        }
    }

    **BUILT_APK_LOCATION**

    bundle {
        language { enableSplit = false }
        density  { enableSplit = false }
        abi      { enableSplit = true  }
    }
}
";
        File.WriteAllText(LAUNCHER_TMPL, content);
        Debug.Log("[BuildFixer] launcherTemplate.gradle criado.");
    }

    // ════════════════════════════════════════════════════════════════════
    // gradleTemplate.properties — suprime warning compileSdk>33
    // ════════════════════════════════════════════════════════════════════

    static void WriteGradleProperties()
    {
        EnsureDir(PLUGINS_DIR);

        string content =
            "# Gerado pelo ChronoMundi GradleBuildFixer\n" +
            "org.gradle.jvmargs=-Xmx4096m\n" +
            "org.gradle.parallel=true\n" +
            "android.useAndroidX=true\n" +
            "android.enableJetifier=true\n" +
            "# Suprime warning do compileSdk > 33 com Gradle plugin 7.4.2\n" +
            "android.suppressUnsupportedCompileSdk=36\n" +
            "**ADDITIONAL_PROPERTIES**\n";

        File.WriteAllText(GRADLE_PROPS, content);
        Debug.Log("[BuildFixer] gradleTemplate.properties criado.");
    }

    // ════════════════════════════════════════════════════════════════════

    static void EnsureDir(string path)
    {
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
    }

    static void Prog(string msg, float p) =>
        EditorUtility.DisplayProgressBar("ChronoMundi — Fix Build", msg, p);
}
#endif