# Guia de Compilação e Geração do Pacote .app (Sem Certificado Pago)

Este documento descreve como gerar o executável iOS no formato `.app` / `.ipa` para instalação via Sideload (AltStore, Sideloadly, TrollStore) sem necessidade de certificado Apple Developer pago.

---

## 1. Visão Geral do Fluxo

```
[ Projeto Unity (port/UnityProject) ]
               │
               ▼ (Unity Build para iOS - Scripting Backend IL2CPP)
[ Projeto Xcode ]
               │
               ▼ (xcodebuild com CODE_SIGNING_ALLOWED=NO)
[ Pacote Unity-iPhone.app ]
               │
               ▼ (Empacotamento)
[ Payload/Unity-iPhone.app -> BombRushCyberfunk.ipa ]
```

---

## 2. Métodos de Instalação no iPhone 12

Como não há certificado de desenvolvedor oficial:

1. **TrollStore (Recomendado se o iPhone 12 estiver no iOS 14.0 - 16.6.1 / 17.0):**
   - Instalação permanente sem expiração de 7 dias.
   - Suporta arquivos `.app` ou `.ipa` diretamente sem necessidade de assinatura.
   - Recursos JIT / AOT sem limitações do sandbox restrito.

2. **Sideloadly / AltStore (Qualquer versão do iOS):**
   - Permite assinar o `.app` usando qualquer Apple ID gratuita comum.
   - O próprio aplicativo no PC/Mac realiza a assinatura ad-hoc localmente no momento da instalação.
   - Validade de 7 dias (renovável via Wi-Fi/cabo).

---

## 3. Comandos de Compilação no macOS (Runner Remoto ou Máquina Local)

Quando o projeto Unity gerar a pasta do projeto Xcode (`build/iOS`):

```bash
# Compilar projeto Xcode gerando o .app sem exigir certificado Apple
xcodebuild clean build \
  -project Unity-iPhone.xcodeproj \
  -scheme Unity-iPhone \
  -configuration Release \
  -sdk iphoneos \
  -destination 'generic/platform=iOS' \
  CODE_SIGNING_ALLOWED=NO \
  CODE_SIGN_IDENTITY="" \
  CODE_SIGNING_REQUIRED=NO \
  CONFIGURATION_BUILD_DIR="./build-output"

# Criar o arquivo .ipa a partir do .app para instaladores padrão
mkdir Payload
cp -r "./build-output/Unity-iPhone.app" Payload/BombRushCyberfunk.app
zip -r BombRushCyberfunk.ipa Payload
```

O arquivo `BombRushCyberfunk.app` (ou `BombRushCyberfunk.ipa`) resultante pode ser transferido diretamente para o iPhone 12.
