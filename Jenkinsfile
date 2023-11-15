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
                withCredentials([usernamePassword(credentialsId: 'DockerHub', usernameVariable: 'wehba', passwordVariable: 'Allordone-12')]){
                    bat "docker login -u $wehba   -p $Allordone-12"
                    bat "docker-compose push"
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