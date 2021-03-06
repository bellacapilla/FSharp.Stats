﻿namespace FSharp.Stats

module Correlation =

    /// Pearson correlation 
    let inline pearson (seq1:seq<'T>) (seq2:seq<'T>) : float =
        use e = seq1.GetEnumerator()
        use e2 = seq2.GetEnumerator()
        let zero = LanguagePrimitives.GenericZero< 'T > 
        let one = LanguagePrimitives.GenericOne<'T> 
        let rec loop n (sumX: 'T) (sumY: 'T) (sumXY: 'T) (sumXX: 'T) (sumYY: 'T) = 
            match (e.MoveNext() && e2.MoveNext()) with
                | true  -> 
                    loop (n + one) (sumX + e.Current) (sumY + e2.Current) (sumXY + (e.Current * e2.Current)) (sumXX + (e.Current * e.Current)) (sumYY + (e2.Current * e2.Current))
                | false -> 
                    if n > zero then  

                        // Covariance
                        let cov = float ((sumXY * n) - (sumX * sumY))

                        // Standard Deviation
                        let stndDev1 = sqrt (float ((n * sumXX) - (sumX * sumX)))
                        let stndDev2 = sqrt (float ((n * sumYY) - (sumY * sumY)))

                        // Correlation
                        cov / (stndDev1 * stndDev2)
                                               
                    else nan
        loop zero zero zero zero zero zero
    
    /// Spearman Correlation (with ranks)
    let spearman array1 array2 =
    
        let spearRank1 = FSharp.Stats.Rank.rankFirst array1 
        let spearRank2 = FSharp.Stats.Rank.rankFirst array2

        pearson spearRank1 spearRank2

    /// Kendall Correlation Coefficient 
    let kendall (setA:_[]) (setB:_[]) =
        let lengthArray = Array.length setA
        let inline kendallCorrFun (setA:_[]) (setB:_[]) =
            let rec loop i j cCon cDisc cTieA cTieB cPairs =      
                if i < lengthArray - 1 then
                    if j <= lengthArray - 1 then
                        if j > i then
                            if (setA.[i] > setA.[j] && setB.[i] > setB.[j]) || (setA.[i] < setA.[j] && setB.[i] < setB.[j]) then
                                loop i (j+1) (cCon + 1.0) cDisc cTieA cTieB (cPairs + 1.0)

                            elif (setA.[i] > setA.[j] && setB.[i] < setB.[j]) || (setA.[i] < setA.[j] && setB.[i] > setB.[j]) then
                                loop i (j+1) cCon (cDisc + 1.0) cTieA cTieB (cPairs + 1.0)

                            else
                                if (setA.[i] = setA.[j]) then
                                    loop i (j+1) cCon cDisc (cTieA + 1.0) cTieB (cPairs + 1.0)

                                else
                                    loop i (j+1) cCon cDisc cTieA (cTieB + 1.0) (cPairs + 1.0)
                        else
                            loop i (j+1) cCon cDisc cTieA cTieB cPairs

                    else 
                        loop (i+1) 1 cCon cDisc cTieA cTieB cPairs

                else
                    let floatLength = lengthArray |> float

                    if (cTieA <> 0.0) || (cTieB <> 0.0) then
                        let n = (floatLength * (floatLength - 1.0)) / 2.0
                        let n1 = (cTieA * (cTieA - 1.0)) / 2.0
                        let n2 = (cTieB * (cTieB - 1.0)) / 2.0
                        (cCon - cDisc) / (sqrt ((n - n1) * (n - n2)))
                
                    else
                        (cCon - cDisc) / ((floatLength * (floatLength - 1.0)) / 2.0)
                
            loop 0 1 0.0 0.0 0.0 0.0 0.0

        kendallCorrFun (FSharp.Stats.Rank.rankFirst setA ) (FSharp.Stats.Rank.rankFirst setB )
