pipeline{
    agent any
    triggers{
        pollSCM("* * * * *")
    }
    stages{
        stage('Build') {
            steps {
                bat "/usr/local/bin/docker compose build"
            }
        }
        stage('Deliver') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'DockerHub', usernameVariable: 'USERNAME', passwordVariable: 'PASSWORD')]){
                    bat "/usr/local/bin/docker login -u $USERNAME -p $PASSWORD"
                    bat "/usr/local/bin/docker compose push"
                }
            }
        }
        stage('Deploy') {
            steps {
                bat "/usr/local/bin/docker compose up --build web-ui"
            }
        }
    }
}