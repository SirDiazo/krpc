load('/tools/build/csharp', 'csharp_binary', 'csharp_library', 'csharp_assembly_info')
load('/tools/build/protobuf/csharp', 'protobuf_csharp')
load('/tools/ServiceDefinitions/build', 'service_definitions')
load('/config', 'version', 'copyright')

csharp_assembly_info(
    name = 'AssemblyInfo',
    title = 'TestServer',
    version = version,
    copyright = copyright
)

csharp_binary(
    name = 'TestServer',
    srcs = glob(['**/*.cs']) + [':AssemblyInfo', '//protobuf:csharp-test'],
    deps = [
        '//server:KRPC',
        '//tools/build/protobuf:Google.Protobuf',
        '//tools/build/ksp:Assembly-CSharp',
        '//tools/build/ksp:Assembly-CSharp-firstpass',
        '//tools/build/ksp:UnityEngine',
        '//tools/build/ksp:TDx.TDxInput',
        '//tools/build/mono-4.5:mscorlib',
        '//tools/build/mono-4.5:System',
        '//tools/build/mono-4.5:System.Core',
        '//tools/build/mono-4.5:System.Runtime',
        '//tools/build/mono-4.5:System.IO',
        '//tools/build/mono-4.5:System.Xml',
        '//tools/build/mono-4.5:System.Xml.Linq',
        '//tools/build/mono-4.5:Newtonsoft.Json'
    ],
    runargs = ['50000', '50001', '--debug'],
    visibility = ['//visibility:public']
)

csharp_library(
    name = 'TestService',
    srcs = glob(['Services/*.cs', 'Properties/*.cs']) + ['//protobuf:csharp-test'],
    deps = [
        '//server:KRPC',
        '//tools/build/protobuf:Google.Protobuf',
        '//tools/build/ksp:Assembly-CSharp',
        '//tools/build/ksp:Assembly-CSharp-firstpass',
        '//tools/build/ksp:UnityEngine',
        '//tools/build/ksp:TDx.TDxInput',
        '//tools/build/mono-4.5:mscorlib',
        '//tools/build/mono-4.5:System',
        '//tools/build/mono-4.5:System.Core',
        '//tools/build/mono-4.5:System.Runtime',
        '//tools/build/mono-4.5:System.IO',
        '//tools/build/mono-4.5:System.Xml',
        '//tools/build/mono-4.5:System.Xml.Linq'
    ]
)

service_definitions(
    name = 'ServiceDefinitions',
    assemblies = [':TestService'],
    service = 'TestService',
    out = 'TestService.json',
    visibility = ['//visibility:public']
)