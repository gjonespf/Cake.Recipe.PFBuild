#!/usr/bin/groovy
def executeXplat(commandString) {
    if (isUnix()) {
        sh commandString
    } else {
        bat commandString
    }
}

pipeline {
    agent { label 'xplat-cake' } 
    environment { 
        CC = 'clang'
    }

    stages {
        stage('Pre') {
            steps
            {
                echo 'Preparing...'
                script {
                    if (isUnix()) {
                        echo 'Running on Unix...'
                        sh "pwsh ./pre.ps1 -t \"Init\"" 
                    } else  {
                        echo 'Running on Windows...'
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './pre.ps1' -Target \"Init\"\""
                    }
                }
            }
        }
        stage('Init') {
            steps
            {
                echo 'Initializing...'
                script {
                    if (isUnix()) {
                        echo 'Running on Unix...'
                        sh "pwsh ./build.ps1 -Target \"Init\"" 
                    } else  {
                        echo 'Running on Windows...'
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Init\"\""
                    }
                }
            }
        }
        stage('Build') {
            steps {
                echo "Running #${env.BUILD_ID} on ${env.JENKINS_URL}"
                echo 'Building...'
                script {
                    if (isUnix()) {
                        sh "pwsh ./build.ps1 -Target \"Build\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Build\"\""
                    }
                }
            }
        }
        stage('Package') {
            steps {
                echo 'Packaging...'
                script {
                    if (isUnix()) {
                        sh "pwsh ./build.ps1 -Target \"Package\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Package\"\""
                    }
                }
            }
        }
        stage('Test'){
            steps {
                echo 'Testing...'
                script {
                    if (isUnix()) {
                        sh "pwsh ./build.ps1 -Target \"Test\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Test\"\""
                    }
                }
            }
        }
        stage('Publish') {
            steps {
                echo 'Publishing...'
                script {
                    if (isUnix()) {
                        sh "pwsh ./build.ps1 -Target \"Publish\"" 
                    } else  {
                        bat "powershell -ExecutionPolicy Bypass -Command \"& './build.ps1' -Target \"Publish\"\""
                    }
                }
            }
        }
    }

    post {
        always {
            archiveArtifacts artifacts: 'BuildArtifacts/**', fingerprint: true
        }
    }
}