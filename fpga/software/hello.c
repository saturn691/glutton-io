#include <stdio.h>
#include <unistd.h>


void looper() 
{
    for (int i = 0; i < 1000; ++i) 
    {
        printf("Hello world, this is the Nios V/m cpu checking in %d...\n", i);
    }
}


int main() 
{
    looper();
    usleep(1000000);
    printf("Bye world!\n");
    fflush(stdout);
    return 0;
}
