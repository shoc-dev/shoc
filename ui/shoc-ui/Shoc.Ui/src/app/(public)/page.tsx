import PublicHero from "./_components/public-hero";
import PublicFeatures from "./_components/public-features";
import PublicFaq from "./_components/public-faq";

export default function PublicHome() {
  return (
    <>
      <PublicHero />
      <PublicFeatures />
      <PublicFaq />
    </>

    // <div className="flex content-center flex-col items-center justify-center text-center ">
    //   <h1 className="text-3xl font-bold mb-4 sm:text-5xl">
    //     Welcome to Shoc Platform.
    //   </h1>
    //   <p className="mb-8 sm:text-xl max-w-[800px] text-muted-foreground">
    //     We build and run your HPC and ML jobs smoothly, fast and secure.
    //   </p>
    //   <div className="flex flex-row items-center gap-5">
    //     <Link
    //       href={`/workspaces`}
    //       className={buttonVariants({ className: "px-6", size: "lg" })}
    //     >
    //       Get Stared
    //     </Link>
    //     <Link
    //       href="/docs"
    //       className={buttonVariants({
    //         variant: "secondary",
    //         className: "px-6",
    //         size: "lg",
    //       })}
    //     >
    //       Docs
    //     </Link>
    //   </div>
    //   <span className="flex flex-row items-start sm:gap-2 gap-0.5 text-muted-foreground text-md mt-7 -mb-12 max-[800px]:mb-12 font-code text-base font-medium">
    //     <TerminalSquareIcon className="w-5 h-5 mr-1 mt-0.5" />
    //     {"npm install -g @shoc/shoc"}
    //   </span>
    // </div>
  );
}