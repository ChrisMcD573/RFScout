dotnet publish src/RFScout.Console/RFScout.Console.csproj \
    -c Release \
    -r linux-arm64 \
    --self-contained true \
    -p:PublishSingleFile=true \
    -p:EnableCompressionInSingleFile=true
