pipeline{
    agent any
    triggers{
        pollSCM("* * * * *")
    }
    stages{
        stage('Build') {
            steps {
                sh "docker compose build"
            }
        }
        stage('Deliver') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'DockerHub', usernameVariable: 'wehba', passwordVariable: 'Allordone-12')]){
                    sh "docker login -u $wehba -p $Allordone-12"
                    sh "docker compose push"
                }
            }
        }
        stage('Deploy') {
            steps {
                sh "/usr/local/bin/docker compose up --build web-ui"
            }
        }
    }
}