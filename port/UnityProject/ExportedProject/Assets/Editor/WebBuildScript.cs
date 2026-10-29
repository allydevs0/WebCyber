#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class WebBuildScript
{
	[MenuItem("BombRush/Build WebGL (WASM)")]
	public static void Build()
	{
		Debug.Log("==================================================");
		Debug.Log("Iniciando Compilação WebGL / WebAssembly (WASM)...");
		Debug.Log("==================================================");

		// Configurações da plataforma WebGL
		EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
		PlayerSettings.WebGL.memorySize = 1024;
		PlayerSettings.WebGL.emscriptenArgs = "-s ALLOW_MEMORY_GROWTH=1";
		PlayerSettings.WebGL.dataCaching = true;
		PlayerSettings.WebGL.decompressionFallback = true;
		PlayerSettings.runInBackground = true;

		// Localizar cenas habilitadas para o build
		string[] scenes = EditorBuildSettings.scenes
			.Where(s => s.enabled)
			.Select(s => s.path)
			.ToArray();

		if (scenes.Length == 0)
		{
			// Fallback para Bootstrap.unity
			string bootstrapPath = "Assets/GameManagement/Scenes/Bootstrap.unity";
			if (File.Exists(bootstrapPath))
			{
				scenes = new string[] { bootstrapPath };
			}
			else
			{
				throw new Exception("Nenhuma cena encontrada para compilação WebGL!");
			}
		}

		string buildOutputDir = "Builds/WebGL";
		string[] args = Environment.GetCommandLineArgs();
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "-customBuildPath" && i + 1 < args.Length)
			{
				buildOutputDir = args[i + 1];
				break;
			}
		}

		if (!Directory.Exists(buildOutputDir))
		{
			Directory.CreateDirectory(buildOutputDir);
		}

		BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
		{
			scenes = scenes,
			locationPathName = buildOutputDir,
			target = BuildTarget.WebGL,
			options = BuildOptions.None
		};

		BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
		BuildSummary summary = report.summary;

		if (summary.result == BuildResult.Succeeded)
		{
			Debug.Log($"Compilação WebGL concluída com sucesso! Tamanho total: {summary.totalSize} bytes em {summary.totalTime.TotalSeconds:F1}s.");
			Debug.Log($"Saída gerada em: {Path.GetFullPath(buildOutputDir)}");
		}
		else
		{
			Debug.LogError($"Falha na compilação WebGL: {summary.result} com {summary.totalErrors} erros.");
			if (Application.isBatchMode)
			{
				EditorApplication.Exit(1);
			}
		}
	}
}
#endif
