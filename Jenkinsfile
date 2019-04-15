#!groovy

def nodeTag = "sp-cake"

def executeXplat(commandString) {
    if (isUnix()) {
        sh commandString
    } else {
        bat commandString
    }
}
def executeXplatGetResult(commandString) {
    if (isUnix()) {
        return sh ( script: commandString, returnStdout: true ).trim()
    } else {
        stdout = bat(returnStdout:true , script: commandString).trim()
        result = stdout.readLines().drop(1).join(" ")       
        return result
    }
}

node(nodeTag) {
//node('sp-cake') {
    environment { 
        BUILD_HOST = 'Jenkins'
        NODE_TAG = nodeTag
    }
    // TODO: Define in common lib?
    def credOctoLocal = "e162c9e0-0792-472d-a01e-51f2b7427f2b"
    def credOctoCloud = "74207640-6946-44f4-8175-171e9d807193"
    def credLocalNuget = "74d7f630-d422-4324-89f2-6ebccc3b3687"
    def credLocalChoco = "74a39230-d94d-4660-b686-daf40f89e462"

    echo "Running on node tag: "+nodeTag

    // pull request or feature branch
    if  (env.BRANCH_NAME != 'master') {
        checkout()
        preinit()
        init()
        build()
        // test whether this is a regular branch build or a merged PR build
        // if (!isPRMergeBuild()) {
        //     codeTest()
        // } 
        // unitTest()
        doPackaging()
        publish()
        doArchiveArtifacts()
    } // master branch / production
    else { 
        checkout()
        preinit()
        init()
        build()
        //allTests()
        // codeTest()
        // unitTest()
        doPackaging()
        publish()
        // Generate releases?
        // Notifies?
        doArchiveArtifacts()
    }
}

def getBaseBranch () {
    def gitexists = fileExists '.git/index'
    if(!gitexists) {
        return "UNKNOWN"
    }
    return executeXplatGetResult('git rev-parse --abbrev-ref HEAD')
}

def isMultibranchBuild() {
    return env.BRANCH_NAME
}

def isPRMergeBuild() {
    if(env.BRANCH_NAME) {
        return (env.BRANCH_NAME.startsWith("PR-") || env.BRANCH_NAME.startsWith("pull-") || env.BRANCH_NAME.startsWith("heads/pull-"))
    }
    def branchName = getBaseBranch()
    if(branchName != "UNKNOWN") {
        return (branchName ==~ /^PR-\d+$/ || branchName ==~ /^pull-\d+$/)
    }
    return false
}
 
def checkout () {
    stage ('Checkout') {
        if(isMultibranchBuild()) {
            def branchName = env.BRANCH_NAME
            echo 'Doing multibranch build for branch "'+ branchName +'"' 
            checkout([
                    $class: 'GitSCM' 
                    , branches:  [[name: branchName]]
                    , doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations
                    , extensions: scm.extensions + [
                        [$class: 'CloneOption', depth: 0, noTags: false, reference: '', shallow: false]
                        ,[$class: 'CleanBeforeCheckout']
                    ]
                    ,userRemoteConfigs: scm.userRemoteConfigs
                ])
        } else {
            echo 'Doing default pipeline build'
            checkout([
                    $class: 'GitSCM'
                    , branches: [[name: 'develop']]
                    , doGenerateSubmoduleConfigurations: scm.doGenerateSubmoduleConfigurations
                    , extensions: scm.extensions + [
                        [$class: 'CloneOption', depth: 0, noTags: false, reference: '', shallow: false]
                        ,[$class: 'LocalBranch', localBranch: '**']
                        ,[$class: 'CleanBeforeCheckout']
                    ]
                    , submoduleCfg: []
                    , honorRefspec: true
                    , userRemoteConfigs: scm.userRemoteConfigs + [[
                        refspec: '+refs/heads/*:refs/remotes/origin/* +refs/pull/*:refs/remotes/pull/*'
                    ]]
                    ])
            //def branchName = getBaseBranch()
        }
        // Run some configs based on properties.json?
        // fileExists 'properties.json'
        // def props = readJSON file: 'properties.json'
    }
}

def preinit () {
    stage ('PreInit') {
        echo 'PreInitializing...'
        if(fileExists('./pre.ps1')) {
            executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./pre.ps1 \" ")
        } else {
            echo "Skipping prep as no script exists"
        }
    }
}

def init () {
    stage ('Init') {
        echo 'Initializing...'
        executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target Init \" ")
    }
}

def build () {
    stage ('Build')
    {
        echo 'Building...'
        executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target Build \" ")
    }
}

def unitTest() {
    stage ('Unit tests') {
        echo 'Running unit tests...'
        executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target UnitTest \" ")
    }
}

def codeTest() {
    stage ('Code tests') {
        echo 'Running code tests...'
        executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target CodeTest \" ")
    }
}

def doPackaging() {
    stage ('Package') {
        echo 'Packaging...'
        executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target Package \" ")
    }
}

def publish() {
    stage ('Publish')
    {
        withCredentials([
            usernamePassword(credentialsId: 'e162c9e0-0792-472d-a01e-51f2b7427f2b', passwordVariable: 'OCTOAPIKEY', usernameVariable: 'OCTOSERVER'),
            usernamePassword(credentialsId: '74207640-6946-44f4-8175-171e9d807193', passwordVariable: 'OCTOCLOUDAPIKEY', usernameVariable: 'OCTOCLOUDSERVER'),
            usernamePassword(credentialsId: '74d7f630-d422-4324-89f2-6ebccc3b3687', passwordVariable: 'LocalNugetApiKey', usernameVariable: 'LocalNugetServerUrl'),
            usernamePassword(credentialsId: '74a39230-d94d-4660-b686-daf40f89e462', passwordVariable: 'LocalChocolateyApiKey', usernameVariable: 'LocalChocolateyServerUrl')
            ]) 
        {
            echo 'Publishing...'
            executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target Publish \" ")
        }
    }
}


def doArchiveArtifacts() {
    stage ('Archive') {
        echo 'Archiving artifacts...'
        //executeXplat("pwsh -ExecutionPolicy Bypass -Command \"& ./build.ps1 -Target Package \" ")
        archiveArtifacts allowEmptyArchive: true, artifacts: 'BuildArtifacts/**', fingerprint: true
    }
}
