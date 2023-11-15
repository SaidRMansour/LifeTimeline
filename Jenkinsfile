pipeline{
    agent any
    triggers{
        pollSCM("* * * * *")
    }
    stages{
        stage('Build') {
            steps {
                bat "docker-compose build"
            }
        }
        stage('Deliver') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'DockerHub', usernameVariable: 'USERNAME', passwordVariable: 'PASSWORD')]){
                    bat "docker login -u $USERNAME -p $PASSWORD"
                    bat "/usr/local/bin/docker compose push"
                }
            }
        }
        stage('Deploy') {
            steps {
                bat "docker-compose up --build web-ui"
            }
        }
    }
}