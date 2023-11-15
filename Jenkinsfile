pipeline{
    agent any
    triggers{
        pollSCM("* * * * *")
    }
    stages{
        stage('Build') {
            steps{                     
                    bat 'docker-compose build'     
                    echo 'Docker-compose-build Build Image Completed'                
                  }  
                     }
       /*  stage('Deliver') {
            steps {
                withCredentials([usernamePassword(credentialsId: 'DockerHub', usernameVariable: 'wehba', passwordVariable: 'Allordone-12')]){
                    bat "/usr/local/bin/docker login -u $wehba   -p $Allordone-12"
                    bat "/usr/local/bin/docker compose push"
                }
            }
        }
        stage('Deploy') {
            steps {
                bat "/usr/local/bin/docker compose up --build web-ui"
            }
        } */
    }
}